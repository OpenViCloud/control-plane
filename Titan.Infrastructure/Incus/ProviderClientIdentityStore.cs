using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Titan.Application.Abstractions;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;
using Titan.Domain.Exceptions;
using Titan.Domain.Repositories.Platform;

namespace Titan.Infrastructure.Incus;

// Generates Titan's client identity on first use and reads it back ever after
// (docs/features/cluster/design.md#provider-client-identity). The private key is encrypted before it reaches
// the aggregate and decrypted only to build the TLS client certificate.
public sealed class ProviderClientIdentityStore(
    IProviderClientIdentityRepository repository, IClusterCredentialCipher cipher, ProviderClientCertificateCache cache)
    : IProviderClientIdentityStore
{
    public async Task<ProviderClientCertificate> GetCertificateAsync(CancellationToken ct)
    {
        ProviderClientIdentity identity = await GetOrCreateAsync(ct);
        return new ProviderClientCertificate(identity.CertificatePem, identity.Fingerprint);
    }

    /// Shared across requests: callers must not dispose it.
    public async Task<X509Certificate2> GetClientCertificateAsync(CancellationToken ct)
    {
        // The row is read every time (one indexed read) so a replaced identity is never served from the cache.
        ProviderClientIdentity identity = await GetOrCreateAsync(ct);
        X509Certificate2? cached = cache.For(identity.Fingerprint);
        if (cached != null)
        {
            return cached;
        }
        string keyPem = DecryptKey(identity);
        using X509Certificate2 withEphemeralKey = X509Certificate2.CreateFromPem(identity.CertificatePem, keyPem);
        // Round-trip through PKCS#12: TLS on macOS refuses an ephemeral key as a client certificate.
        X509Certificate2 loaded = X509CertificateLoader.LoadPkcs12(withEphemeralKey.Export(X509ContentType.Pkcs12), null);
        return cache.Keep(identity.Fingerprint, loaded);
    }

    /// Only for the worker's connection route (docs/features/cluster/design.md#worker-cluster-connection): the private
    /// key leaves the control plane here and nowhere else.
    public async Task<ProviderClientKeyPair> GetKeyPairAsync(CancellationToken ct)
    {
        ProviderClientIdentity identity = await GetOrCreateAsync(ct);
        return new ProviderClientKeyPair(identity.CertificatePem, DecryptKey(identity));
    }

    private string DecryptKey(ProviderClientIdentity identity)
    {
        try
        {
            return cipher.Decrypt(identity.KeyCipher);
        }
        catch (CryptographicException)
        {
            throw Unreadable();
        }
        catch (ArgumentException)
        {
            // A blob too short to hold nonce and tag: the row is corrupted.
            throw Unreadable();
        }
    }

    private static ServiceUnavailableException Unreadable() =>
        new("CLUSTER_IDENTITY_UNREADABLE",
            "Titan's client key cannot be decrypted with the current Encryption__ClusterKey (key changed, or the stored key is corrupted).");

    private async Task<ProviderClientIdentity> GetOrCreateAsync(CancellationToken ct)
    {
        ProviderClientIdentity? stored = await repository.FindAsync(ct);
        if (stored != null)
        {
            return stored;
        }

        ProviderClientIdentity generated = Generate();
        bool weStoredIt = await repository.TryAddAsync(generated, ct);
        if (weStoredIt)
        {
            return generated;
        }
        // Another control plane stored one first under the same fixed id: everyone uses that one.
        ProviderClientIdentity? winner = await repository.FindAsync(ct);
        if (winner == null)
        {
            throw new InvalidOperationException("The provider client identity was neither stored nor found.");
        }
        return winner;
    }

    private ProviderClientIdentity Generate()
    {
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var request = new CertificateRequest("CN=titan", key, HashAlgorithmName.SHA384);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.2")], critical: false));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        using X509Certificate2 cert = request.CreateSelfSigned(now.AddMinutes(-5), now.AddYears(10));

        string certificatePem = cert.ExportCertificatePem() + "\n";
        string fingerprint = Convert.ToHexStringLower(SHA256.HashData(cert.RawData));
        byte[] keyCipher = cipher.Encrypt(key.ExportPkcs8PrivateKeyPem());
        return ProviderClientIdentity.Create(certificatePem, keyCipher, fingerprint);
    }
}

public record ProviderClientKeyPair(string CertificatePem, string KeyPem);

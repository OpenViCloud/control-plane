using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Titan.Infrastructure.Incus;

namespace Titan.Tests.Infrastructure.Incus;

// Titan's client identity toward Incus (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#provider-client-identity): generated
// once by the control plane, then the same certificate forever — a new one would leave every cluster that trusts the
// old one refusing Titan, with nothing pointing at the cause.
public class ProviderClientIdentityStoreTests
{
    [Fact]
    public async Task FirstUse_GeneratesAnEcdsaP384ClientCertificate()
    {
        var store = IncusTestKit.IdentityStore();

        X509Certificate2 cert = await store.GetClientCertificateAsync(CancellationToken.None);

        Assert.True(cert.HasPrivateKey);
        using ECDsa? key = cert.GetECDsaPublicKey();
        Assert.NotNull(key);
        Assert.Equal(384, key.KeySize);
        var eku = cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().Single();
        Assert.Contains(eku.EnhancedKeyUsages.Cast<Oid>(), o => o.Value == "1.3.6.1.5.5.7.3.2");
    }

    [Fact]
    public async Task EveryRead_ReturnsTheSameCertificate_AndGeneratesOnce()
    {
        var repository = new StubIdentityRepository();
        var store = IncusTestKit.IdentityStore(repository);

        var first = await store.GetCertificateAsync(CancellationToken.None);
        var second = await store.GetCertificateAsync(CancellationToken.None);
        // A fresh store over the same rows stands in for a restarted control plane.
        var afterRestart = await IncusTestKit.IdentityStore(repository).GetCertificateAsync(CancellationToken.None);

        Assert.Equal(first.Fingerprint, second.Fingerprint);
        Assert.Equal(first.Fingerprint, afterRestart.Fingerprint);
        Assert.Equal(1, repository.Adds);
    }

    [Fact]
    public async Task TheFingerprint_IsTheSha256OfTheCertificate()
    {
        var store = IncusTestKit.IdentityStore();

        var publicHalf = await store.GetCertificateAsync(CancellationToken.None);

        using var cert = X509Certificate2.CreateFromPem(publicHalf.CertificatePem);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(cert.RawData)), publicHalf.Fingerprint);
    }

    [Fact]
    public async Task ThePublicHalf_CarriesNoPrivateKey_AndTheStoredKeyIsEncrypted()
    {
        var repository = new StubIdentityRepository();
        var store = IncusTestKit.IdentityStore(repository);

        var publicHalf = await store.GetCertificateAsync(CancellationToken.None);

        Assert.DoesNotContain("PRIVATE KEY", publicHalf.CertificatePem);
        string storedKeyBytes = System.Text.Encoding.UTF8.GetString(repository.Stored!.KeyCipher);
        Assert.DoesNotContain("PRIVATE KEY", storedKeyBytes);
    }

    // Two control planes starting at once: the one that loses the insert uses the winner's identity.
    [Fact]
    public async Task LosingTheInsertRace_UsesTheWinnersIdentity()
    {
        var winnerRepository = new StubIdentityRepository();
        var winner = await IncusTestKit.IdentityStore(winnerRepository).GetCertificateAsync(CancellationToken.None);
        var loserRepository = new StubIdentityRepository { WinnerOfARace = winnerRepository.Stored };

        var loser = await IncusTestKit.IdentityStore(loserRepository).GetCertificateAsync(CancellationToken.None);
        using X509Certificate2 loserClientCert =
            await IncusTestKit.IdentityStore(loserRepository).GetClientCertificateAsync(CancellationToken.None);

        Assert.Equal(winner.Fingerprint, loser.Fingerprint);
        Assert.Equal(winner.Fingerprint, Convert.ToHexStringLower(SHA256.HashData(loserClientCert.RawData)));
    }

    // Built once per process: every catalog request would otherwise decrypt the key and load a new certificate.
    [Fact]
    public async Task TheClientCertificate_IsBuiltOnce_AndShared()
    {
        var repository = new StubIdentityRepository();
        var cache = new ProviderClientCertificateCache();
        var store = new Titan.Infrastructure.Incus.ProviderClientIdentityStore(repository, IncusTestKit.Cipher(), cache);

        X509Certificate2 first = await store.GetClientCertificateAsync(CancellationToken.None);
        X509Certificate2 second = await new Titan.Infrastructure.Incus.ProviderClientIdentityStore(
            repository, IncusTestKit.Cipher(), cache).GetClientCertificateAsync(CancellationToken.None);

        Assert.Same(first, second);
    }

    // A replaced identity row (database restore, a deliberate reset) must not leave the process presenting the old
    // certificate while the admin page shows the new one.
    [Fact]
    public async Task AReplacedIdentityRow_IsNotServedFromTheOldCertificate()
    {
        var repository = new StubIdentityRepository();
        var cache = new ProviderClientCertificateCache();
        var store = new Titan.Infrastructure.Incus.ProviderClientIdentityStore(repository, IncusTestKit.Cipher(), cache);
        X509Certificate2 before = await store.GetClientCertificateAsync(CancellationToken.None);
        repository.Stored = null;
        var replacement = await IncusTestKit.IdentityStore(repository).GetCertificateAsync(CancellationToken.None);

        X509Certificate2 after = await store.GetClientCertificateAsync(CancellationToken.None);

        Assert.NotEqual(Convert.ToHexStringLower(SHA256.HashData(before.RawData)), replacement.Fingerprint);
        Assert.Equal(replacement.Fingerprint, Convert.ToHexStringLower(SHA256.HashData(after.RawData)));
    }

    // A truncated or tampered key blob is a named failure, not a 500.
    [Fact]
    public async Task ACorruptedKeyBlob_IsANamedFailure()
    {
        var repository = new StubIdentityRepository();
        await IncusTestKit.IdentityStore(repository).GetCertificateAsync(CancellationToken.None);
        var stored = repository.Stored!;
        repository.Stored = Titan.Domain.Aggregates.Platform.ProviderClientIdentity.ProviderClientIdentity.Create(
            stored.CertificatePem, [1, 2, 3], stored.Fingerprint);

        var error = await Assert.ThrowsAsync<Titan.Domain.Exceptions.ServiceUnavailableException>(() =>
            IncusTestKit.IdentityStore(repository).GetClientCertificateAsync(CancellationToken.None));

        Assert.Equal("CLUSTER_IDENTITY_UNREADABLE", error.Code);
    }
}

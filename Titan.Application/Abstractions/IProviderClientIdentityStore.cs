namespace Titan.Application.Abstractions;

// The public half of Titan's client identity: what an admin adds to a cluster's trust store
// (docs/features/cluster/design.md#provider-client-identity). The private key never leaves Infrastructure.
public record ProviderClientCertificate(string CertificatePem, string Fingerprint);

public interface IProviderClientIdentityStore
{
    /// The same certificate on every call: generated once, then read back.
    Task<ProviderClientCertificate> GetCertificateAsync(CancellationToken ct);
}

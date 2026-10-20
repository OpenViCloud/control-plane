namespace Titan.Application.Queries.Compute.SshKey.Contracts;

/// The public key text itself is not returned: the fingerprint identifies it, and nothing reads the key back.
public record SshKeyQueryResult(Guid Id, string Name, string Type, string Fingerprint, DateTimeOffset CreatedAt);

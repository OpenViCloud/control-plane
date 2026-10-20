using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;

namespace Titan.Domain.Repositories.Platform;

public interface IProviderClientIdentityRepository
{
    /// Null until the control plane has generated its identity.
    Task<ProviderClientIdentity?> FindAsync(CancellationToken ct);

    /// False when another writer stored the identity first (same fixed id): the caller reads that one back.
    Task<bool> TryAddAsync(ProviderClientIdentity identity, CancellationToken ct);
}

using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Domain.Repositories.Platform;

public interface IClusterRepository
{
    /// Null when the zone has no cluster. Untracked.
    Task<ClusterAggregate?> FindByZoneIdAsync(string zoneId, CancellationToken ct);

    Task<IReadOnlyList<ClusterAggregate>> GetAllAsync(CancellationToken ct);

    /// Tracked, so a caller can mutate the returned aggregate and have SaveChangesAsync persist it.
    Task<ClusterAggregate?> GetForUpdateAsync(string zoneId, CancellationToken ct);

    Task AddAsync(ClusterAggregate cluster, CancellationToken ct);

    /// Pass an aggregate from GetForUpdateAsync so the change tracker can delete it.
    Task RemoveAsync(ClusterAggregate cluster, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}

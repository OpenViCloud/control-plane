using Microsoft.EntityFrameworkCore;
using Titan.Domain.Repositories.Platform;
using Titan.Infrastructure.Persistence;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Infrastructure.Repositories;

public class ClusterRepository(TitanDbContext db) : IClusterRepository
{
    public Task<ClusterAggregate?> FindByZoneIdAsync(string zoneId, CancellationToken ct) =>
        db.Clusters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ZoneId == zoneId, ct);

    public async Task<IReadOnlyList<ClusterAggregate>> GetAllAsync(CancellationToken ct) =>
        await db.Clusters
            .AsNoTracking()
            .OrderBy(c => c.ZoneId)
            .ToListAsync(ct);

    public Task<ClusterAggregate?> GetForUpdateAsync(string zoneId, CancellationToken ct) =>
        db.Clusters.FirstOrDefaultAsync(c => c.ZoneId == zoneId, ct);

    public async Task AddAsync(ClusterAggregate cluster, CancellationToken ct) =>
        await db.Clusters.AddAsync(cluster, ct);

    public Task RemoveAsync(ClusterAggregate cluster, CancellationToken ct)
    {
        db.Clusters.Remove(cluster);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

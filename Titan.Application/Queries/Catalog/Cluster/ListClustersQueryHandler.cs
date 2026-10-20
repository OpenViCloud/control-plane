using Titan.Application.Abstractions;
using Titan.Application.Options;
using Titan.Application.Queries.Catalog.Cluster.Contracts;

namespace Titan.Application.Queries.Catalog.Cluster;

public class ListClustersQueryHandler(IComputeCatalogProvider provider, ZoneOptions zoneOptions)
{
    public async Task<List<ClusterInfo>> HandleAsync(ListClustersQuery query, CancellationToken ct = default)
    {
        var zoneId = query.ZoneId;

        var zone = zoneOptions.Zones.FirstOrDefault(z => z.Id == zoneId);
        if (zone is null)
            throw new KeyNotFoundException($"Zone '{zoneId}' not found.");
        return await provider.ListClustersAsync(zoneId, ct);
    }
}

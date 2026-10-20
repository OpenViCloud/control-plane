using Titan.Application.Abstractions;
using Titan.Application.Options;
using Titan.Application.Queries.Catalog.Zone.Contracts;

namespace Titan.Application.Queries.Catalog.Zone;

public class ListZonesQueryHandler(ZoneOptions zoneOptions)
{
    public Task<List<ZoneInfo>> HandleAsync(ListZonesQuery query, CancellationToken ct = default)
    {
        var zones = zoneOptions.Zones
            .Select(z => new ZoneInfo(z.Id, z.Name))
            .ToList();
        return Task.FromResult(zones);
    }
}

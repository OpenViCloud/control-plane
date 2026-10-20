using FastEndpoints;
using Titan.Application.Abstractions;
using Titan.Application.Queries.Catalog.Zone;
using Titan.Application.Queries.Catalog.Zone.Contracts;

namespace Titan.Infrastructure.Endpoints.Catalog;

public class ListZonesEndpoint(ListZonesQueryHandler handler) : EndpointWithoutRequest<List<ZoneInfo>>
{
    public override void Configure()
    {
        Get("v1/zones");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListZonesQuery(), ct), cancellation: ct);
    }
}

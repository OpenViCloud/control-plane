using FastEndpoints;
using Titan.Application.Abstractions;
using Titan.Application.Queries.Catalog.Cluster;
using Titan.Application.Queries.Catalog.Cluster.Contracts;

namespace Titan.Infrastructure.Endpoints.Catalog;

public class ListClustersRequest
{
    public string ZoneId { get; set; } = "";
}

public class ListClustersEndpoint(ListClustersQueryHandler handler) : Endpoint<ListClustersRequest, List<ClusterInfo>>
{
    public override void Configure()
    {
        Get("v1/zones/{zoneId}/clusters");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListClustersRequest req, CancellationToken ct)
    {
        req.ZoneId = Route<string>("zoneId") ?? "";
        try
        {
            var clusters = await handler.HandleAsync(new ListClustersQuery(req.ZoneId), ct);
            await SendAsync(clusters, cancellation: ct);
        }
        catch (KeyNotFoundException)
        {
            await SendNotFoundAsync(ct);
        }
    }
}

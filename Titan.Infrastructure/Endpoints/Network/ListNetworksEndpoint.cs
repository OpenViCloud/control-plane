using FastEndpoints;
using Titan.Application.Queries.Networking.Network;
using Titan.Application.Queries.Networking.Network.Contracts;

namespace Titan.Infrastructure.Endpoints.Network;

public class ListNetworksEndpoint(ListNetworksQueryHandler handler) : EndpointWithoutRequest<List<NetworkQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/networks");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListNetworksQuery(), ct), cancellation: ct);
    }
}

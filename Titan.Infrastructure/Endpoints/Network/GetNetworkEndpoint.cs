using FastEndpoints;
using Titan.Application.Queries.Networking.Network;
using Titan.Application.Queries.Networking.Network.Contracts;

namespace Titan.Infrastructure.Endpoints.Network;

public class GetNetworkEndpoint(GetNetworkQueryHandler handler) : EndpointWithoutRequest<NetworkQueryResult>
{
    public override void Configure()
    {
        Get("v1/tenant/networks/{networkId:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var networkId = Route<Guid>("networkId");
        await SendAsync(await handler.HandleAsync(new GetNetworkQuery(networkId), ct), cancellation: ct);
    }
}

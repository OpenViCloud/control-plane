using FastEndpoints;
using Titan.Application.Queries.Compute.LoadBalancer;
using Titan.Application.Queries.Compute.LoadBalancer.Contracts;

namespace Titan.Infrastructure.Endpoints.LoadBalancer;

public class GetLoadBalancerEndpoint(GetLoadBalancerQueryHandler handler) : EndpointWithoutRequest<LoadBalancerQueryResult>
{
    public override void Configure()
    {
        Get("v1/tenant/load-balancers/{loadBalancerId:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var loadBalancerId = Route<Guid>("loadBalancerId");
        await SendAsync(await handler.HandleAsync(new GetLoadBalancerQuery(loadBalancerId), ct), cancellation: ct);
    }
}

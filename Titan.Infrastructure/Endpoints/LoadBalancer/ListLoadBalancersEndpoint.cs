using FastEndpoints;
using Titan.Application.Queries.Compute.LoadBalancer;
using Titan.Application.Queries.Compute.LoadBalancer.Contracts;

namespace Titan.Infrastructure.Endpoints.LoadBalancer;

public class ListLoadBalancersEndpoint(ListLoadBalancersQueryHandler handler)
    : EndpointWithoutRequest<List<LoadBalancerQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/load-balancers");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListLoadBalancersQuery(), ct), cancellation: ct);
    }
}

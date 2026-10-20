using FastEndpoints;
using Titan.Application.Queries.Compute.Vm;
using Titan.Application.Queries.Compute.Vm.Contracts;

namespace Titan.Infrastructure.Endpoints.Vm;

public class ListVmsEndpoint(ListVmsQueryHandler handler) : EndpointWithoutRequest<List<VmQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/vms");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListVmsQuery(), ct), cancellation: ct);
    }
}

using FastEndpoints;
using Titan.Application.Queries.Compute.Vm;
using Titan.Application.Queries.Compute.Vm.Contracts;

namespace Titan.Infrastructure.Endpoints.Vm;

public class GetVmEndpoint(GetVmQueryHandler handler) : EndpointWithoutRequest<VmQueryResult>
{
    public override void Configure()
    {
        Get("v1/tenant/vms/{vmId:guid}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var vmId = Route<Guid>("vmId");
        await SendAsync(await handler.HandleAsync(new GetVmQuery(vmId), ct), cancellation: ct);
    }
}

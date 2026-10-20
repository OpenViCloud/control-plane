using FastEndpoints;
using Titan.Application.Queries.Compute.VmSnapshot;
using Titan.Application.Queries.Compute.VmSnapshot.Contracts;

namespace Titan.Infrastructure.Endpoints.VmSnapshots;

public class ListSnapshotsEndpoint(ListVmSnapshotsQueryHandler handler)
    : EndpointWithoutRequest<List<VmSnapshotQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/vms/{vmId:guid}/snapshots");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var vmId = Route<Guid>("vmId");
        var result = await handler.HandleAsync(new ListVmSnapshotsQuery(vmId), ct);
        await SendAsync(result, 200, ct);
    }
}

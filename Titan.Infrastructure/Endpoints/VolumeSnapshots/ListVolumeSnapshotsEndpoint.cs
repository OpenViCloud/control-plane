using FastEndpoints;
using Titan.Application.Queries.Compute.VolumeSnapshot;
using Titan.Application.Queries.Compute.VolumeSnapshot.Contracts;

namespace Titan.Infrastructure.Endpoints.VolumeSnapshots;

public class ListVolumeSnapshotsEndpoint(ListVolumeSnapshotsQueryHandler handler)
    : EndpointWithoutRequest<List<VolumeSnapshotQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/volumes/{volumeId:guid}/snapshots");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var volumeId = Route<Guid>("volumeId");
        var result = await handler.HandleAsync(new ListVolumeSnapshotsQuery(volumeId), ct);
        await SendAsync(result, 200, ct);
    }
}

using FastEndpoints;
using Titan.Application.Queries.Compute.Volume;
using Titan.Application.Queries.Compute.Volume.Contracts;

namespace Titan.Infrastructure.Endpoints.Volume;

public class ListVolumesEndpoint(ListVolumesQueryHandler handler)
    : EndpointWithoutRequest<List<VolumeQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/volumes");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await handler.HandleAsync(new ListVolumesQuery(), ct), cancellation: ct);
    }
}

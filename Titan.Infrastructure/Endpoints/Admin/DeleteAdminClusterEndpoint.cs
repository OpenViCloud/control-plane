using FastEndpoints;
using Titan.Application.Abstractions;

namespace Titan.Infrastructure.Endpoints.Admin;

// The zone is then unregistered: its catalog answers 404 CLUSTER_NOT_FOUND (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#cluster-connection-fallback).
// Covered by AdminClusterServiceTests.
public class DeleteAdminClusterEndpoint(IAdminClusterService clusters) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("v1/admin/clusters/{zoneId}");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await clusters.DeleteAsync(Route<string>("zoneId")!, ct);
        await SendNoContentAsync(ct);
    }
}

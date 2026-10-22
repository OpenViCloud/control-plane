using FastEndpoints;
using Titan.Application.Queries.Admin.Network;
using Titan.Application.Queries.Admin.Network.Contracts;

namespace Titan.Infrastructure.Endpoints.Admin;

// https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/tenancy-and-admin.md#admin-cross-tenant-read. Optional ?tenantId= narrows to one owner.
public class ListAdminNetworksEndpoint(ListAdminNetworksQueryHandler handler) : EndpointWithoutRequest<List<AdminNetworkQueryResult>>
{
    public override void Configure()
    {
        Get("v1/admin/networks");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tenantId = Query<Guid?>("tenantId", isRequired: false);
        await SendAsync(await handler.HandleAsync(new ListAdminNetworksQuery(tenantId), ct), cancellation: ct);
    }
}

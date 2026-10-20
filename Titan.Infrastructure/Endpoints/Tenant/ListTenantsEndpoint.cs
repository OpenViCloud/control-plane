using FastEndpoints;
using Titan.Application.Queries.Platform.Tenant;
using Titan.Application.Queries.Platform.Tenant.Contracts;

namespace Titan.Infrastructure.Endpoints.Tenant;

public class ListTenantsEndpoint(ListTenantsQueryHandler listTenants) : EndpointWithoutRequest<List<AdminTenantQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenants");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await listTenants.HandleAsync(new ListTenantsQuery(), ct), cancellation: ct);
    }
}

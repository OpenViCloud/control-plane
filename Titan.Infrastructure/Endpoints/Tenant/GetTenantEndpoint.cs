using FastEndpoints;
using Titan.Application.Queries.Platform.Tenant;
using Titan.Application.Queries.Platform.Tenant.Contracts;

namespace Titan.Infrastructure.Endpoints.Tenant;

public class GetTenantEndpoint(GetTenantQueryHandler getTenant) : EndpointWithoutRequest<TenantQueryResult>
{
    public override void Configure()
    {
        Get("v1/tenant");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await getTenant.HandleAsync(new GetTenantQuery(), ct), cancellation: ct);
    }
}

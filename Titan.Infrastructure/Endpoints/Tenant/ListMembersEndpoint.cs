using FastEndpoints;
using Titan.Application.Queries.Platform.Tenant;
using Titan.Application.Queries.Platform.Tenant.Contracts;

namespace Titan.Infrastructure.Endpoints.Tenant;

public class ListMembersEndpoint(ListTenantMembersQueryHandler listMembers) : EndpointWithoutRequest<List<TenantMemberQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/members");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(await listMembers.HandleAsync(new ListTenantMembersQuery(), ct), cancellation: ct);
    }
}

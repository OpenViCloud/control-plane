using FastEndpoints;
using Titan.Application.Queries.Networking.FirewallRule;
using Titan.Application.Queries.Networking.FirewallRule.Contracts;

namespace Titan.Infrastructure.Endpoints.Network;

public class ListFirewallRulesEndpoint(ListFirewallRulesQueryHandler handler) : EndpointWithoutRequest<List<FirewallRuleQueryResult>>
{
    public override void Configure()
    {
        Get("v1/tenant/networks/{networkId:guid}/firewall-rules");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var networkId = Route<Guid>("networkId");
        var result = await handler.HandleAsync(new ListFirewallRulesQuery(networkId), ct);
        await SendOkAsync(result, ct);
    }
}

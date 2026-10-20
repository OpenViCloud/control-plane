using Microsoft.AspNetCore.Http;
using FastEndpoints;
using Titan.Application.Commands.Networking.FirewallRule;
using Titan.Application.Commands.Networking.FirewallRule.Contracts;

namespace Titan.Infrastructure.Endpoints.Network;

public class DeleteFirewallRuleEndpoint(DeleteFirewallRuleCommandHandler handler) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("v1/tenant/networks/{networkId:guid}/firewall-rules/{ruleId:guid}");
        Description(b => b.ClearDefaultProduces(200).Produces(204));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var networkId = Route<Guid>("networkId");
        var ruleId = Route<Guid>("ruleId");
        await handler.HandleAsync(new DeleteFirewallRuleCommand(networkId, ruleId), ct);
        await SendNoContentAsync(ct);
    }
}

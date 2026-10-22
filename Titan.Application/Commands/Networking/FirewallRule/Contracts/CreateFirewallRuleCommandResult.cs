namespace Titan.Application.Commands.Networking.FirewallRule.Contracts;

// https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/http-contract.md#create-returns-id: a create answers with the identity it just minted, nothing more.
public record CreateFirewallRuleCommandResult(Guid RuleId);

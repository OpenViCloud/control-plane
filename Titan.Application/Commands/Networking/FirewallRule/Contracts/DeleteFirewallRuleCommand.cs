namespace Titan.Application.Commands.Networking.FirewallRule.Contracts;

public record DeleteFirewallRuleCommand(Guid NetworkId, Guid RuleId);

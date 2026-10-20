namespace Titan.Application.Queries.Networking.FirewallRule.Contracts;

// The firewall reconciler asks for the rules of one zone's networks it must observe.
public record ListReconcilableFirewallRulesQuery(string ZoneId);

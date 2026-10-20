namespace Titan.Domain.Aggregates.Networking.FirewallRule;

/// The request still waiting for the provider, derived — never stored.
public enum FirewallRuleTransition
{
    Creating,
    Deleting
}

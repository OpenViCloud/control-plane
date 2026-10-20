namespace Titan.Application.Queries.Compute.LoadBalancer.Contracts;

// What the reconciler needs to find the load balancer on Incus (its network, in its project, by VIP) and what Titan
// last recorded, so only a difference is reported.
public record ReconcilableLoadBalancerQueryResult(
    Guid Id, string ProjectName, string ExternalNetworkId, string Vip, string CurrentExistence);

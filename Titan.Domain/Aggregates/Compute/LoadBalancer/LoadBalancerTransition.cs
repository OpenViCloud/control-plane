namespace Titan.Domain.Aggregates.Compute.LoadBalancer;

/// The request waiting on Incus, derived, never stored (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/load-balancer/design.md#lb-lifecycle).
public enum LoadBalancerTransition
{
    Creating,
    Updating,
    Deleting
}

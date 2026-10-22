namespace Titan.Domain.Aggregates.Compute.LoadBalancer;

/// What a listener carries: Incus OVN load balancers pass TCP and UDP through, nothing at layer 7
/// (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/load-balancer/design.md#lb-scope). Stored by name.
public enum LoadBalancerProtocol
{
    TCP,
    UDP
}

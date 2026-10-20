namespace Titan.Application.Commands.Compute.LoadBalancer.Contracts;

// The route carries the id; the body (UpdateLoadBalancerBody, next to the endpoint) the whole configuration.
public record UpdateLoadBalancerCommand(
    Guid LoadBalancerId, List<LoadBalancerListenerInput> Listeners, List<LoadBalancerBackendInput> Backends);

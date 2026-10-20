namespace Titan.Application.Commands.Compute.LoadBalancer.Contracts;

// The command IS the wire contract (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/code-structure.md#command-is-wire-contract).
public record CreateLoadBalancerCommand(
    string Name, Guid NetworkId, List<LoadBalancerListenerInput> Listeners, List<LoadBalancerBackendInput> Backends);

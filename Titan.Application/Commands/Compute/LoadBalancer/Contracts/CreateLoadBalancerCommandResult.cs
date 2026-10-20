namespace Titan.Application.Commands.Compute.LoadBalancer.Contracts;

// Only the id (docs/architecture/http-contract.md#create-returns-id).
public record CreateLoadBalancerCommandResult(Guid LoadBalancerId);

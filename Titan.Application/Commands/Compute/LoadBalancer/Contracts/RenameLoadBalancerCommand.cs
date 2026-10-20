namespace Titan.Application.Commands.Compute.LoadBalancer.Contracts;

public record RenameLoadBalancerCommand(Guid LoadBalancerId, string Name);

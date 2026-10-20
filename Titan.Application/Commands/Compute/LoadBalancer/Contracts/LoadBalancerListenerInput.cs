namespace Titan.Application.Commands.Compute.LoadBalancer.Contracts;

/// Protocol is TCP or UDP; the validator refuses anything else.
public record LoadBalancerListenerInput(string Protocol, int Port, int TargetPort);

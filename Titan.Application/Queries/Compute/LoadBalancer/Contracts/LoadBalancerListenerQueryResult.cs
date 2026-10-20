namespace Titan.Application.Queries.Compute.LoadBalancer.Contracts;

public record LoadBalancerListenerQueryResult(string Protocol, int Port, int TargetPort);

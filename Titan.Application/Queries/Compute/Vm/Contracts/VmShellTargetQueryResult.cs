namespace Titan.Application.Queries.Compute.Vm.Contracts;

// Where an SSH session execs: the zone picks the cluster, then the Incus project and instance.
public record VmShellTargetQueryResult(string ZoneId, string ProjectName, string InstanceName);

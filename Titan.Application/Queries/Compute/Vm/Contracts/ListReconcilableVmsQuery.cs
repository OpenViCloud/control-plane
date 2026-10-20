namespace Titan.Application.Queries.Compute.Vm.Contracts;

// The VM reconciler asks for the VMs of one zone it must observe.
public record ListReconcilableVmsQuery(string ZoneId);

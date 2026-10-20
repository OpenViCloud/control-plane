namespace Titan.Application.Queries.Compute.VmSnapshot.Contracts;

// The VM snapshot reconciler asks for the snapshots of one zone's VMs it must observe.
public record ListReconcilableVmSnapshotsQuery(string ZoneId);

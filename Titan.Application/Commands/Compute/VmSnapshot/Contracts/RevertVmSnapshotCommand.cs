namespace Titan.Application.Commands.Compute.VmSnapshot.Contracts;

public record RevertVmSnapshotCommand(Guid VmId, Guid SnapshotId);

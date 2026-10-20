namespace Titan.Application.Commands.Compute.VmSnapshot.Contracts;

public record DeleteVmSnapshotCommand(Guid VmId, Guid SnapshotId);

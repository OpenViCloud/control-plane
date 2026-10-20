namespace Titan.Application.Commands.Compute.VolumeSnapshot.Contracts;

public record RestoreVolumeSnapshotCommand(Guid VolumeId, Guid SnapshotId, string Name);

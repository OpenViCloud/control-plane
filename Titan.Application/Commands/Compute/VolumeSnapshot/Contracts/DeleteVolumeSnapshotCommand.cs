namespace Titan.Application.Commands.Compute.VolumeSnapshot.Contracts;

public record DeleteVolumeSnapshotCommand(Guid VolumeId, Guid SnapshotId);

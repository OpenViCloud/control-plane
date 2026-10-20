namespace Titan.Application.Commands.Compute.VolumeSnapshot.Contracts;

public record CreateVolumeSnapshotCommand(Guid VolumeId, string Name);

namespace Titan.Application.Commands.Compute.VolumeSnapshot.Contracts;

// Only the id (docs/architecture/http-contract.md#create-returns-id).
public record CreateVolumeSnapshotCommandResult(Guid SnapshotId);

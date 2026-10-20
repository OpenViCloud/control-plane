namespace Titan.Application.Commands.Compute.VolumeSnapshot.Contracts;

// Only the id (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#create-returns-id).
public record CreateVolumeSnapshotCommandResult(Guid SnapshotId);

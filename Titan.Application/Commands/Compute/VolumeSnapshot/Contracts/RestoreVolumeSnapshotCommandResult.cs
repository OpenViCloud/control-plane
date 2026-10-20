namespace Titan.Application.Commands.Compute.VolumeSnapshot.Contracts;

// Restore mints a NEW volume, so by https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#create-returns-id its id is exactly what
// comes back.
public record RestoreVolumeSnapshotCommandResult(Guid NewVolumeId);

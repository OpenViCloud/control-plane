namespace Titan.Application.Commands.Compute.Volume.Contracts;

// Only the id (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/http-contract.md#create-returns-id).
public record CreateVolumeCommandResult(Guid VolumeId);

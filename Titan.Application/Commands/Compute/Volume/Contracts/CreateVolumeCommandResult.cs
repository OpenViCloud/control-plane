namespace Titan.Application.Commands.Compute.Volume.Contracts;

// Only the id (docs/architecture/http-contract.md#create-returns-id).
public record CreateVolumeCommandResult(Guid VolumeId);

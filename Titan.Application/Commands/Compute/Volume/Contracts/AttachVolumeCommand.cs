namespace Titan.Application.Commands.Compute.Volume.Contracts;

// {volumeId} comes from the route, VmId from the body — the endpoint composes both (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/code-structure.md#command-is-wire-contract).
public record AttachVolumeCommand(Guid VolumeId, Guid VmId);

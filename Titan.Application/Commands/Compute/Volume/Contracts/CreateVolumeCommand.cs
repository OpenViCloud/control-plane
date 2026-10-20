namespace Titan.Application.Commands.Compute.Volume.Contracts;

// The command IS the wire contract (docs/architecture/code-structure.md#command-is-wire-contract): the endpoint binds the request body straight to it.
public record CreateVolumeCommand(string Name, string VolumeOfferingId, string ZoneId, int SizeGb);

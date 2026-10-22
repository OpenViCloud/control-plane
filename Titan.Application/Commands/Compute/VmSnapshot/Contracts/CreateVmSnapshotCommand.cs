namespace Titan.Application.Commands.Compute.VmSnapshot.Contracts;

// {vmId} from the route, Name from the body — the endpoint composes both (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/code-structure.md#command-is-wire-contract).
public record CreateVmSnapshotCommand(Guid VmId, string Name);

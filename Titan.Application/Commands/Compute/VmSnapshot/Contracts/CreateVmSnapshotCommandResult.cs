namespace Titan.Application.Commands.Compute.VmSnapshot.Contracts;

// Only the id (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#create-returns-id), so there is no query shape to collide with
// (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/code-structure.md#no-contract-reuse).
public record CreateVmSnapshotCommandResult(Guid SnapshotId);

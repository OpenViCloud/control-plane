namespace Titan.Application.Commands.Compute.VmSnapshot.Contracts;

// Only the id (docs/architecture/http-contract.md#create-returns-id), so there is no query shape to collide with
// (docs/architecture/code-structure.md#no-contract-reuse).
public record CreateVmSnapshotCommandResult(Guid SnapshotId);

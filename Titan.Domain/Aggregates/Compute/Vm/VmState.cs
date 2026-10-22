namespace Titan.Domain.Aggregates.Compute.Vm;

/// The Incus instance status, word for word (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/desired-and-current.md#provider-state-is-verbatim):
/// exactly the five a QEMU instance reports (incus driver_qemu.go statusCode). Only an observation writes it;
/// Vm.CurrentState is null while Incus has no instance. Whether the instance exists is Vm.CurrentExistence.
public enum VmState
{
    Running,
    Ready,
    Stopped,
    Frozen,
    Error
}

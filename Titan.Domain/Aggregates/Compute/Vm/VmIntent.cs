namespace Titan.Domain.Aggregates.Compute.Vm;

/// Derived, not stored (https://github.com/OpenViCloud/titan/blob/main/docs/features/vm/design.md#pending-intent). Observation methods return it so the
/// caller records each request's outcome once.
public enum VmIntent
{
    Create,
    Stop,
    Start,
    Delete
}

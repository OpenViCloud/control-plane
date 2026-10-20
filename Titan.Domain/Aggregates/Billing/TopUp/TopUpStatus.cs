namespace Titan.Domain.Aggregates.Billing.TopUp;

/// Stored by member name (docs/architecture/code-structure.md#enum-storage): inserting a member mid-enum
/// never silently re-maps money-path rows.
public enum TopUpStatus
{
    Pending,
    Confirmed,
    Failed
}

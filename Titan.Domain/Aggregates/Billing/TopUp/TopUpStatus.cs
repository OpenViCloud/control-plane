namespace Titan.Domain.Aggregates.Billing.TopUp;

/// Stored by member name (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/code-structure.md#enum-storage): inserting a member mid-enum
/// never silently re-maps money-path rows.
public enum TopUpStatus
{
    Pending,
    Confirmed,
    Failed
}

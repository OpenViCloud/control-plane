namespace Titan.Domain.Aggregates.Billing.Invoice;

/// Never persisted (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/code-structure.md#enum-storage). A separate type from
/// <see cref="InvoiceStatus"/> so assigning a display label to the stored column is a compile error.
public enum InvoiceDisplayStatus
{
    New,
    Paid,
    Settled,
    Overdue
}

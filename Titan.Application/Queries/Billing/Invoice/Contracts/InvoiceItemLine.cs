using Titan.Domain.Aggregates.Billing.Invoice;
using Titan.Domain.Aggregates.Billing.UsageRecord;

namespace Titan.Application.Queries.Billing.Invoice.Contracts;

public record InvoiceItemLine(string ResourceType, Guid ResourceId, string ResourceName, decimal Amount, string Currency)
{
    public static InvoiceItemLine From(InvoiceItem item) =>
        // The wire keeps the string token; the enum is a C#-side detail.
        new(item.ResourceType.ToDbString(), item.ResourceId, item.ResourceName, item.Amount, item.Currency);
}

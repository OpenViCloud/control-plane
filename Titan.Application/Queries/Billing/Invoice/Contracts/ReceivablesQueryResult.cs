namespace Titan.Application.Queries.Billing.Invoice.Contracts;

// Accounts receivable, admin view (docs/architecture/money-and-wallet.md#postpaid-no-brake). Class names unique
// across the contract (docs/architecture/http-contract.md#openapi-schema-names). "Overdue" is derived (New past
// DueAt), never a stored status.
public record ReceivablesQueryResult(
    decimal TotalOutstanding,
    decimal TotalOverdue,
    int OverdueInvoiceCount,
    int TenantsInDebt,
    string Currency,
    IReadOnlyList<TenantReceivableLine> PerTenant);

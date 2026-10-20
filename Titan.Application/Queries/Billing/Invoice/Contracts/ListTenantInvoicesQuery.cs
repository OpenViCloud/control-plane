namespace Titan.Application.Queries.Billing.Invoice.Contracts;

// Tenant comes from the caller's context (docs/architecture/http-contract.md#tenant-scoping Style B), so the query carries no parameters.
public record ListTenantInvoicesQuery();

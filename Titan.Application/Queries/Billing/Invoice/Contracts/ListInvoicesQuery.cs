namespace Titan.Application.Queries.Billing.Invoice.Contracts;

// Status filters on the DERIVED display status (e.g. "Overdue"); null = every invoice.
public record ListInvoicesQuery(string? Status);

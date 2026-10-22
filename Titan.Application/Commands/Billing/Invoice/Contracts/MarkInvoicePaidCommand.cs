namespace Titan.Application.Commands.Billing.Invoice.Contracts;

// Money that arrived OUTSIDE Titan (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/money-and-wallet.md#human-confirmed-debit) — the wallet is never touched.
public record MarkInvoicePaidCommand(Guid InvoiceId);

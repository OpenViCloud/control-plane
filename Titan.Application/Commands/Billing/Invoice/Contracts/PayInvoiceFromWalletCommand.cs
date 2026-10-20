namespace Titan.Application.Commands.Billing.Invoice.Contracts;

// https://github.com/OpenViCloud/titan/blob/main/docs/architecture/money-and-wallet.md#human-confirmed-debit — the tenant pays one of its own invoices from its wallet.
public record PayInvoiceFromWalletCommand(Guid InvoiceId);

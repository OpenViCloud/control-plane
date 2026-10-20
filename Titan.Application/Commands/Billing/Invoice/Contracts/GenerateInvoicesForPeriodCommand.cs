namespace Titan.Application.Commands.Billing.Invoice.Contracts;

// The endpoint resolves a missing period to the month that just closed.
public record GenerateInvoicesForPeriodCommand(int Year, int Month);

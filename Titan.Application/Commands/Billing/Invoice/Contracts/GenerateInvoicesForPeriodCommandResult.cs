namespace Titan.Application.Commands.Billing.Invoice.Contracts;

// Not a create of one thing and not an update: a batch run whose counts are the answer the CronJob
// logs.
public record GenerateInvoicesForPeriodCommandResult(int Generated, int Skipped);

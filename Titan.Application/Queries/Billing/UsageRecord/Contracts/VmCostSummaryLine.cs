namespace Titan.Application.Queries.Billing.UsageRecord.Contracts;

public record VmCostSummaryLine(Guid VmId, string VmName, decimal TotalCost);

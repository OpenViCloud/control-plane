namespace Titan.Application.Queries.Billing.UsageRecord.Contracts;

public record TenantBillingSummaryQueryResult(
    string Month, decimal TotalCost, string Currency,
    decimal ComputeCost, decimal StorageCost,
    IReadOnlyList<VmCostSummaryLine> PerVm);

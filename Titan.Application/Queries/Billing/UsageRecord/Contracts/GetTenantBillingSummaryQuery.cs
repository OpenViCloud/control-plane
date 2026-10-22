namespace Titan.Application.Queries.Billing.UsageRecord.Contracts;

// Tenant comes from the caller's context (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/http-contract.md#tenant-scoping Style B); the period from ?month=YYYY-MM.
public record GetTenantBillingSummaryQuery(int Year, int Month);

namespace Titan.Application.Queries.Billing.UsageRecord.Contracts;

// The endpoint has already parsed ?month=YYYY-MM and checked the VM belongs to the caller.
public record GetVmCostQuery(Guid VmId, int Year, int Month);

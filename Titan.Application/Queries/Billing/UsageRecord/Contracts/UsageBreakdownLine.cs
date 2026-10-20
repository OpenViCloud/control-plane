namespace Titan.Application.Queries.Billing.UsageRecord.Contracts;

/// One append-only usage record's contribution to a VM's bill — one row per bucket it existed in.
public record UsageBreakdownLine(DateTimeOffset Bucket, string UsageType, decimal Cost);

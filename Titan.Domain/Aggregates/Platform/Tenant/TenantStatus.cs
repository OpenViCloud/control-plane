namespace Titan.Domain.Aggregates.Platform.Tenant;

public enum TenantStatus
{
    // Signup awaiting approval: an unverified email, or a platform-admin decision.
    PendingApproval,
    Active,
    Suspended
}

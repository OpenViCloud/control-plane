namespace Titan.Application.Queries.Admin.Network.Contracts;

// Cross-tenant (docs/architecture/tenancy-and-admin.md#admin-cross-tenant-read). TenantId null = every tenant; set = narrow to one owner.
public record ListAdminNetworksQuery(Guid? TenantId);

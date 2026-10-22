namespace Titan.Application.Queries.Platform.Tenant.Contracts;

// Tenant comes from the caller's context (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/http-contract.md#tenant-scoping Style B), so the query carries no parameters.
public record GetTenantQuery();

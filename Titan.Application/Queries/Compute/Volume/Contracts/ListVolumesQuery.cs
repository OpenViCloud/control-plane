namespace Titan.Application.Queries.Compute.Volume.Contracts;

// Tenant comes from the caller's context (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#tenant-scoping Style B), so the query carries no parameters.
public record ListVolumesQuery();

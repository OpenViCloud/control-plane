namespace Titan.Application.Queries.Networking.Network.Contracts;

// The caller's own networks — the tenant comes from the caller, not from a parameter (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/http-contract.md#tenant-scoping).
public record ListNetworksQuery();

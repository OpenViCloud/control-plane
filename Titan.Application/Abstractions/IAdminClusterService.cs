using Titan.Domain.Aggregates.Platform.Cluster;

namespace Titan.Application.Abstractions;

// Nothing here is secret: an Incus cluster is reached with Titan's own client identity, not a stored credential
// (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-incus). Source is "db" (a stored row) or "none" (a configured
// zone nobody has registered a cluster for, which has no Id/CreatedAt/UpdatedAt).
public record AdminClusterDto(
    Guid? Id,
    string ZoneId,
    string Name,
    string Endpoint,
    string ServerFingerprint,
    string Source,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    AdminClusterProviderSettingsDto ProviderSettings);

// Null = not set (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-provider-settings).
public record AdminClusterProviderSettingsDto(string? OvnUplinkNetwork, string? StoragePool, string? Ipv6Block)
{
    public static AdminClusterProviderSettingsDto From(ClusterProviderSettings s) =>
        new(s.OvnUplinkNetwork, s.StoragePool, s.Ipv6Block);
}

public interface IAdminClusterService
{
    Task<List<AdminClusterDto>> ListForAdminAsync(CancellationToken ct);

    /// Upsert by zone.
    Task UpsertAsync(string zoneId, string? name, string endpoint, string serverFingerprint, CancellationToken ct);

    /// Throws NotFoundException("CLUSTER_NOT_FOUND") when the zone has no stored row.
    Task DeleteAsync(string zoneId, CancellationToken ct);

    /// Throws NotFoundException("CLUSTER_NOT_FOUND") when the zone has no stored row.
    Task SetProviderSettingsAsync(string zoneId, ClusterProviderSettings settings, CancellationToken ct);
}

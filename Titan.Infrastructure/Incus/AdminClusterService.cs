using Titan.Application.Abstractions;
using Titan.Application.Options;
using Titan.Domain.Aggregates.Platform.Cluster;
using Titan.Domain.Exceptions;
using Titan.Domain.Repositories.Platform;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Infrastructure.Incus;

public class AdminClusterService(IClusterRepository repository, ZoneOptions zones) : IAdminClusterService
{
    // One entry per configured zone — its row, or "none" so the UI knows where to PUT — then any row whose zone is
    // no longer configured, so nothing Titan stores goes unseen.
    public async Task<List<AdminClusterDto>> ListForAdminAsync(CancellationToken ct)
    {
        var rows = await repository.GetAllAsync(ct);
        var rowsByZone = rows.ToDictionary(r => r.ZoneId);
        var list = new List<AdminClusterDto>();

        foreach (var zone in zones.Zones)
        {
            bool zoneHasARow = rowsByZone.TryGetValue(zone.Id, out ClusterAggregate? row);
            if (zoneHasARow)
            {
                list.Add(ToDto(row!));
                continue;
            }
            list.Add(NotRegistered(zone.Id));
        }

        var configuredZoneIds = zones.Zones.Select(z => z.Id).ToHashSet();
        foreach (var row in rows)
        {
            bool zoneIsConfigured = configuredZoneIds.Contains(row.ZoneId);
            if (!zoneIsConfigured)
            {
                list.Add(ToDto(row));
            }
        }
        return list;
    }

    public async Task UpsertAsync(
        string zoneId, string? name, string endpoint, string serverFingerprint, CancellationToken ct)
    {
        var existing = await repository.GetForUpdateAsync(zoneId, ct);
        if (existing != null)
        {
            existing.UpdateConnection(name, endpoint, serverFingerprint);
            await repository.SaveChangesAsync(ct);
            return;
        }

        var created = ClusterAggregate.Create(zoneId, name, endpoint, serverFingerprint);
        await repository.AddAsync(created, ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task SetProviderSettingsAsync(string zoneId, ClusterProviderSettings settings, CancellationToken ct)
    {
        var existing = await repository.GetForUpdateAsync(zoneId, ct);
        if (existing == null)
        {
            throw new NotFoundException("CLUSTER_NOT_FOUND", "No cluster is registered for this zone.");
        }
        existing.SetProviderSettings(settings);
        await repository.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string zoneId, CancellationToken ct)
    {
        var existing = await repository.GetForUpdateAsync(zoneId, ct);
        if (existing == null)
        {
            // 404 rather than a silent 204 so a wrong zoneId surfaces.
            throw new NotFoundException("CLUSTER_NOT_FOUND", "No cluster is registered for this zone.");
        }
        await repository.RemoveAsync(existing, ct);
        await repository.SaveChangesAsync(ct);
    }

    private static AdminClusterDto NotRegistered(string zoneId) =>
        new(Id: null, zoneId, Name: zoneId, Endpoint: "", ServerFingerprint: "", Source: "none",
            CreatedAt: null, UpdatedAt: null, AdminClusterProviderSettingsDto.From(ClusterProviderSettings.None));

    private static AdminClusterDto ToDto(ClusterAggregate c) => new(
        c.Id, c.ZoneId, c.Name, c.Endpoint, c.ServerFingerprint, Source: "db", c.CreatedAt, c.UpdatedAt,
        AdminClusterProviderSettingsDto.From(c.ProviderSettings));
}

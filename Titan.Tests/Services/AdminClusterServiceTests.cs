using Titan.Application.Options;
using Titan.Domain.Aggregates.Platform.Cluster;
using Titan.Domain.Exceptions;
using Titan.Infrastructure.Incus;
using Titan.Tests.Infrastructure.Incus;

namespace Titan.Tests.Services;

// An Incus cluster row holds nothing secret (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-incus). The rules with
// weight: every configured zone shows, registered or not, so the admin sees where to register; a row whose zone left
// the config still shows; there is no config fallback any more.
public class AdminClusterServiceTests
{
    private const string Fingerprint = "ffcf7803b2371e90834c2dd2a454f48bcb9c541b16dd7dc54a93a104b1aa108f";

    private static readonly ZoneOptions Zones = new()
    {
        Zones = [new ZoneConfig { Id = "mock", Name = "mock" }, new ZoneConfig { Id = "eude01", Name = "eude01" }],
    };

    private static (AdminClusterService Service, StubClusterRepository Rows) Build()
    {
        var rows = new StubClusterRepository();
        return (new AdminClusterService(rows, Zones), rows);
    }

    [Fact]
    public async Task List_NoRows_ShowsEveryZoneAsNotRegistered()
    {
        var (service, _) = Build();

        var list = await service.ListForAdminAsync(CancellationToken.None);

        Assert.Equal(["mock", "eude01"], list.Select(c => c.ZoneId).ToList());
        Assert.All(list, c => Assert.Equal("none", c.Source));
        Assert.All(list, c => Assert.Equal("", c.Endpoint));
        Assert.All(list, c => Assert.Null(c.Id));
    }

    [Fact]
    public async Task List_ARegisteredZone_ShowsItsEndpointAndFingerprint()
    {
        var (service, rows) = Build();
        rows.Rows.Add(Cluster.Create("mock", "incus-lab", "https://incus-lab:8443", Fingerprint));

        var list = await service.ListForAdminAsync(CancellationToken.None);

        var mock = list.Single(c => c.ZoneId == "mock");
        Assert.Equal("db", mock.Source);
        Assert.Equal("incus-lab", mock.Name);
        Assert.Equal("https://incus-lab:8443", mock.Endpoint);
        Assert.Equal(Fingerprint, mock.ServerFingerprint);
        Assert.Equal("none", list.Single(c => c.ZoneId == "eude01").Source);
    }

    [Fact]
    public async Task List_ARowWhoseZoneIsNoLongerConfigured_StillShows()
    {
        var (service, rows) = Build();
        rows.Rows.Add(Cluster.Create("retired", null, "https://old:8443", Fingerprint));

        var list = await service.ListForAdminAsync(CancellationToken.None);

        Assert.Contains(list, c => c.ZoneId == "retired" && c.Source == "db");
    }

    [Fact]
    public async Task Upsert_FirstSave_CreatesTheRow_WithTheCanonicalFingerprint()
    {
        var (service, rows) = Build();

        await service.UpsertAsync("mock", "incus-lab", "https://incus-lab:8443/", Fingerprint.ToUpperInvariant(), CancellationToken.None);

        var row = Assert.Single(rows.Rows);
        Assert.Equal("https://incus-lab:8443", row.Endpoint);
        Assert.Equal(Fingerprint, row.ServerFingerprint);
        Assert.Equal(1, rows.Saves);
    }

    [Fact]
    public async Task Upsert_Again_UpdatesTheSameRow()
    {
        var (service, rows) = Build();
        await service.UpsertAsync("mock", "old", "https://old:8443", Fingerprint, CancellationToken.None);
        string other = new('a', 64);

        await service.UpsertAsync("mock", "new", "https://new:8443", other, CancellationToken.None);

        var row = Assert.Single(rows.Rows);
        Assert.Equal("new", row.Name);
        Assert.Equal("https://new:8443", row.Endpoint);
        Assert.Equal(other, row.ServerFingerprint);
    }

    [Fact]
    public async Task Delete_RemovesTheRow()
    {
        var (service, rows) = Build();
        rows.Rows.Add(Cluster.Create("mock", null, "https://incus-lab:8443", Fingerprint));

        await service.DeleteAsync("mock", CancellationToken.None);

        Assert.Empty(rows.Rows);
    }

    [Fact]
    public async Task Delete_AZoneWithoutARow_IsNotFound()
    {
        var (service, _) = Build();

        var error = await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync("mock", CancellationToken.None));

        Assert.Equal("CLUSTER_NOT_FOUND", error.Code);
    }

    [Fact]
    public async Task SetProviderSettings_ReplacesThemWhole()
    {
        var (service, rows) = Build();
        rows.Rows.Add(Cluster.Create("mock", null, "https://incus-lab:8443", Fingerprint));
        await service.SetProviderSettingsAsync("mock", ClusterProviderSettings.Create("ovnup", "default", null), CancellationToken.None);

        await service.SetProviderSettingsAsync("mock", ClusterProviderSettings.Create("ovnup", null, null), CancellationToken.None);

        var settings = rows.Rows.Single().ProviderSettings;
        Assert.Equal("ovnup", settings.OvnUplinkNetwork);
        Assert.Null(settings.StoragePool);
    }

    [Fact]
    public async Task SetProviderSettings_AZoneWithoutARow_IsNotFound()
    {
        var (service, _) = Build();

        var error = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SetProviderSettingsAsync("mock", ClusterProviderSettings.None, CancellationToken.None));

        Assert.Equal("CLUSTER_NOT_FOUND", error.Code);
    }
}

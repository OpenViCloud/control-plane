using Titan.Domain.Exceptions;
using Titan.Infrastructure.Incus;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Tests.Infrastructure.Incus;

// The catalog reads a zone's Incus cluster live (docs/features/cluster/design.md#cluster-incus). Bodies are
// the lab recordings, which is where the two traps come from: a single server's /1.0/cluster has server_name "", and an
// untrusted client gets 200 with an empty image list — a catalog that looks healthy and is not.
public class IncusCatalogProviderTests
{
    private static async Task<(IncusCatalogProvider Catalog, StubClusterRepository Rows, string TitanFingerprint)> Catalog(
        params string[] zones)
    {
        var store = IncusTestKit.IdentityStore();
        var titan = await store.GetCertificateAsync(CancellationToken.None);
        var rows = new StubClusterRepository();
        var catalog = new IncusCatalogProvider(rows, new IncusClientFactory(store), IncusTestKit.Zones(zones),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<IncusCatalogProvider>.Instance);
        return (catalog, rows, titan.Fingerprint);
    }

    [Fact]
    public async Task ASingleServer_IsOneClusterNamedAfterTheServer()
    {
        var (catalog, rows, titan) = await Catalog("mock");
        await using var server = await FakeIncusServer.StartAsync(titan);
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, server.Fingerprint));

        var clusters = await catalog.ListClustersAsync("mock");

        var only = Assert.Single(clusters);
        Assert.Equal("lima-incus-lab", only.Name);
        Assert.Equal("lima-incus-lab", only.Id);
    }

    // No lab recording of a clustered Incus yet: the member list shape is the documented one (a list of member URLs).
    [Fact]
    public async Task AClusteredServer_ListsItsMembers()
    {
        var (catalog, rows, titan) = await Catalog("mock");
        await using var server = await FakeIncusServer.StartAsync(titan);
        server.TrustedOverrides["/1.0/cluster"] = (200,
            """{"type":"sync","status":"Success","status_code":200,"operation":"","error_code":0,"error":"","metadata":{"server_name":"node1","enabled":true,"member_config":[]}}""");
        server.TrustedOverrides["/1.0/cluster/members"] = (200,
            """{"type":"sync","status":"Success","status_code":200,"operation":"","error_code":0,"error":"","metadata":["/1.0/cluster/members/node1","/1.0/cluster/members/node2"]}""");
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, server.Fingerprint));

        var clusters = await catalog.ListClustersAsync("mock");

        Assert.Equal(["node1", "node2"], clusters.Select(c => c.Name).ToList());
    }

    [Fact]
    public async Task AClusterThatDoesNotTrustTitan_IsAnError_NotAnEmptyCatalog()
    {
        var (catalog, rows, _) = await Catalog("mock");
        await using var server = await FakeIncusServer.StartAsync();
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, server.Fingerprint));

        var clusters = await Assert.ThrowsAsync<ServiceUnavailableException>(() => catalog.ListClustersAsync("mock"));
        var images = await Assert.ThrowsAsync<ServiceUnavailableException>(() => catalog.ListOsImagesAsync());

        Assert.Equal("CLUSTER_UNTRUSTED", clusters.Code);
        Assert.Equal("CLUSTER_UNTRUSTED", images.Code);
    }

    [Fact]
    public async Task AServerThatIsNotThePinnedOne_IsAnError()
    {
        var (catalog, rows, titan) = await Catalog("mock");
        await using var server = await FakeIncusServer.StartAsync(titan);
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, new string('a', 64)));

        var error = await Assert.ThrowsAsync<ServiceUnavailableException>(() => catalog.ListClustersAsync("mock"));

        Assert.Equal("CLUSTER_FINGERPRINT_MISMATCH", error.Code);
    }

    [Fact]
    public async Task AZoneWithoutACluster_IsNotFound()
    {
        var (catalog, _, _) = await Catalog("mock");

        var error = await Assert.ThrowsAsync<NotFoundException>(() => catalog.ListClustersAsync("mock"));

        Assert.Equal("CLUSTER_NOT_FOUND", error.Code);
    }

    // The lab holds one VM image and one container image of Ubuntu noble; Titan only sells VMs.
    [Fact]
    public async Task OsImages_AreTheVirtualMachineImages_OfTheFirstZone()
    {
        var (catalog, rows, titan) = await Catalog("mock", "eude01");
        await using var server = await FakeIncusServer.StartAsync(titan);
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, server.Fingerprint));

        var images = await catalog.ListOsImagesAsync();

        var only = Assert.Single(images);
        Assert.Equal("b89557e2fde36cf2492fcca65f8df6975a5a0e220cd386e2f86d69940db0bb3b", only.Id);
        Assert.Equal("Ubuntu noble arm64 (20261006_07:42)", only.Name);
        Assert.Equal(1, only.SizeGb); // 353,603,748 bytes, rounded up: an image never fits in less than itself
    }

    // The catalog routes are anonymous: the error names what is wrong, never the internal endpoint or fingerprints.
    [Fact]
    public async Task AnAnonymousCaller_SeesNoInternalEndpointOrFingerprint()
    {
        var (catalog, rows, titan) = await Catalog("mock");
        await using var server = await FakeIncusServer.StartAsync();
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, server.Fingerprint));

        var error = await Assert.ThrowsAsync<ServiceUnavailableException>(() => catalog.ListClustersAsync("mock"));

        Assert.Equal("CLUSTER_UNTRUSTED", error.Code);
        Assert.DoesNotContain(server.Endpoint, error.Message);
        Assert.DoesNotContain(server.Fingerprint, error.Message);
        Assert.DoesNotContain(titan, error.Message);
    }

    [Fact]
    public async Task AnAnswerThatIsNotIncusShaped_IsAClusterError()
    {
        var (catalog, rows, titan) = await Catalog("mock");
        await using var server = await FakeIncusServer.StartAsync(titan);
        server.TrustedOverrides["/1.0/cluster"] = (200, """{"type":"sync","metadata":{"enabled":"yes"}}""");
        rows.Rows.Add(ClusterAggregate.Create("mock", null, server.Endpoint, server.Fingerprint));

        var error = await Assert.ThrowsAsync<ServiceUnavailableException>(() => catalog.ListClustersAsync("mock"));

        Assert.Equal("CLUSTER_ERROR", error.Code);
    }
}

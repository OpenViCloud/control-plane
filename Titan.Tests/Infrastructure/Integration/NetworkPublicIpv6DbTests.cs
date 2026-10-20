using Microsoft.EntityFrameworkCore;
using Titan.Domain.Aggregates.Networking.Network;
using Titan.Domain.Aggregates.Platform.Tenant;
using Titan.Infrastructure.Endpoints;
using Titan.Infrastructure.Persistence;
using Titan.Infrastructure.Repositories;

namespace Titan.Tests.Infrastructure.Integration;

/// One public /64 per live network of a zone (docs/infra/incus/design.md#ipv6-address-plan). Two creates that picked
/// the same free /64 at once both pass the handler; only the unique index stops the second, and ErrorStatus turns
/// that into 409 IPV6_ALLOCATION_CONFLICT by the index name. InMemory enforces no index, so only Postgres shows it.
/// Category=Integration — its own CI step; needs real Postgres.
[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class NetworkPublicIpv6DbTests(PostgresFixture pg)
{
    private const string Block = "2001:41d0:d00:d400::/56";

    private static Network WithSlash64(Tenant tenant, string zone, int index) =>
        Network.Create(tenant, 0, 0, $"net-{Guid.NewGuid():N}", zone, NetworkPublicIpv6.For(Block, index));

    private async Task SaveAsync(Network network)
    {
        await using TitanDbContext db = pg.NewDbContext();
        db.Networks.Add(network);
        await db.SaveChangesAsync();
    }

    private async Task<HashSet<int>> TakenAsync(string zone)
    {
        await using TitanDbContext db = pg.NewDbContext();
        return await new NetworkRepository(db).ListPublicIpv6IndexesInZoneAsync(zone);
    }

    [Fact]
    public async Task TwoNetworksOfOneZone_OnTheSameSlash64_TheSecondIs409Ipv6AllocationConflict()
    {
        string zone = $"zone-{Guid.NewGuid():N}";
        await SaveAsync(WithSlash64(TestTenant.Active("a", "kc-a", "a@a.com"), zone, 7));

        var ex = await Assert.ThrowsAsync<DbUpdateException>(
            () => SaveAsync(WithSlash64(TestTenant.Active("b", "kc-b", "b@b.com"), zone, 7)));

        Assert.Equal(new ErrorStatus(409, ErrorStatus.Ipv6AllocationConflictCode), ErrorStatus.For(ex));
    }

    [Fact]
    public async Task TheSameSlash64Number_InTwoZones_IsFine()
    {
        var tenant = TestTenant.Active("a", "kc-a", "a@a.com");

        await SaveAsync(WithSlash64(tenant, $"zone-{Guid.NewGuid():N}", 7));
        await SaveAsync(WithSlash64(tenant, $"zone-{Guid.NewGuid():N}", 7));
    }

    // The index and the handler's query agree: a network the delete command confirmed gone frees its /64, and one
    // whose create gave up before Incus had it held none.
    [Fact]
    public async Task ADeletedNetwork_AndAFailedCreate_HoldNoSlash64()
    {
        string zone = $"zone-{Guid.NewGuid():N}";
        var tenant = TestTenant.Active("a", "kc-a", "a@a.com");
        Network gone = WithSlash64(tenant, zone, 3);
        gone.MarkCreated("net-gone", "10.0.0.0/24", "10.0.0.1");
        gone.MarkObserved(NetworkStatus.Created);
        gone.RequestDelete();
        gone.MarkDeleted();
        Network failed = WithSlash64(tenant, zone, 4);
        failed.MarkRequestFailed("Network creation failed.", null);
        Network live = WithSlash64(tenant, zone, 5);
        await SaveAsync(gone);
        await SaveAsync(failed);
        await SaveAsync(live);

        Assert.Equal(new HashSet<int> { 5 }, await TakenAsync(zone));
        await SaveAsync(WithSlash64(tenant, zone, 3));
        await SaveAsync(WithSlash64(tenant, zone, 4));
    }

    // The allocator's other input (NetworkPublicIpv6.FirstFree): per number, the latest time a deleted network of the
    // zone gave it back. Live networks and other zones are not in it.
    [Fact]
    public async Task Releases_AreTheLatestPerNumber_OfDeletedNetworksOfTheZone()
    {
        string zone = $"zone-{Guid.NewGuid():N}";
        var tenant = TestTenant.Active("a", "kc-a", "a@a.com");
        Network first = Deleted(WithSlash64(tenant, zone, 3));
        await SaveAsync(first);
        await Task.Delay(20);
        Network second = Deleted(WithSlash64(tenant, zone, 3));
        await SaveAsync(second);
        await SaveAsync(WithSlash64(tenant, zone, 5));
        await SaveAsync(Deleted(WithSlash64(tenant, $"zone-{Guid.NewGuid():N}", 6)));

        await using TitanDbContext db = pg.NewDbContext();
        Dictionary<int, DateTimeOffset?> released = await new NetworkRepository(db).ListPublicIpv6ReleasesInZoneAsync(zone);

        Assert.Equal(new[] { 3 }, released.Keys.OrderBy(k => k).ToArray());
        Assert.NotNull(released[3]);
        Assert.Equal(second.PublicIpv6ReleasedAt!.Value, released[3]!.Value, TimeSpan.FromMilliseconds(1));
        Assert.True(released[3] > first.PublicIpv6ReleasedAt);
    }

    private static Network Deleted(Network network)
    {
        network.MarkCreated($"net-{Guid.NewGuid():N}", "10.0.0.0/24", "10.0.0.1");
        network.MarkObserved(NetworkStatus.Created);
        network.RequestDelete();
        network.MarkDeleted();
        return network;
    }
}

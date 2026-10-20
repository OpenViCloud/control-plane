using System.Text;
using Microsoft.EntityFrameworkCore;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;
using Titan.Infrastructure.Persistence;
using Titan.Infrastructure.Repositories;

namespace Titan.Tests.Infrastructure.Integration;

/// Postgres behaviour of rows the shared fixture cannot hold for one test alone: the single provider client identity
/// and the seeded catalog price. Each test gets its own empty database, migrated to the latest schema.
/// Category=Integration — its own CI step; needs real Postgres.
[Trait("Category", "Integration")]
public sealed class OwnDatabaseDbTests : IAsyncLifetime
{
    // More decimals than the catalog once kept (numeric(10,6) rounded it to 0.000088); the column is numeric(18,10).
    private const decimal PrecisionProbe = 0.0000877300m;

    private readonly TestPostgres _container = new();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = NewDbContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private TitanDbContext NewDbContext() =>
        new(new DbContextOptionsBuilder<TitanDbContext>().UseNpgsql(_container.GetConnectionString()).Options);

    private static string Certificate(string marker) =>
        $"-----BEGIN CERTIFICATE-----\n{marker}\n-----END CERTIFICATE-----\n";

    // Two control planes starting at once must end with one identity (docs/features/cluster/design.md#provider-client-identity):
    // a second one would make every cluster that trusts the first refuse Titan.
    [Fact]
    public async Task TwoWritersStoringTheIdentity_EndWithOneRow_AndTheLoserIsToldSo()
    {
        var first = ProviderClientIdentity.Create(Certificate("first"), Encoding.UTF8.GetBytes("k1"), new string('1', 64));
        var second = ProviderClientIdentity.Create(Certificate("second"), Encoding.UTF8.GetBytes("k2"), new string('2', 64));

        bool firstStored;
        bool secondStored;
        await using (var db = NewDbContext())
        {
            firstStored = await new ProviderClientIdentityRepository(db).TryAddAsync(first, CancellationToken.None);
        }
        await using (var db = NewDbContext())
        {
            secondStored = await new ProviderClientIdentityRepository(db).TryAddAsync(second, CancellationToken.None);
        }

        Assert.True(firstStored);
        Assert.False(secondStored);
        await using (var db = NewDbContext())
        {
            var only = await db.ProviderClientIdentities.AsNoTracking().SingleAsync();
            Assert.Equal(new string('1', 64), only.Fingerprint);
        }
    }

    // The listed price is kept at the precision usage is billed at (docs/architecture/money-and-wallet.md#rate-value-object).
    [Fact]
    public async Task ACatalogPriceWithTenDecimals_SurvivesTheRoundTrip()
    {
        await using (var db = NewDbContext())
        {
            await db.Database.ExecuteSqlAsync($"update core.vm_offerings set price_per_hour = {PrecisionProbe} where id = 'm'");
        }

        await using (var db = NewDbContext())
        {
            var stored = await db.VmOfferings.AsNoTracking().SingleAsync(o => o.Id == "m");
            Assert.Equal(PrecisionProbe, stored.PricePerHour.PerUnit);
        }
    }

    // The read path goes through the Rate value converter: the cost of an interval from the stored rate must equal the
    // cost from the rate the admin approved.
    [Fact]
    public async Task CostFromTheStoredRate_MatchesCostFromTheApprovedRate()
    {
        await using var read = NewDbContext();
        var stored = await read.VmOfferings.AsNoTracking().SingleAsync(o => o.Id == "m");

        const decimal oneTenMinuteInterval = 10m / 60m;
        var approved = 311.256m;    // the approved number, written here not read from seed

        Assert.Equal(approved, stored.PricePerHour.PerUnit);
        Assert.Equal(approved * oneTenMinuteInterval, stored.PricePerHour * oneTenMinuteInterval);
    }
}

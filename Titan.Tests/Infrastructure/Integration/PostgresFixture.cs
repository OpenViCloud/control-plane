using Microsoft.EntityFrameworkCore;
using Titan.Infrastructure.Persistence;

namespace Titan.Tests.Infrastructure.Integration;

/// One real Postgres per test collection. TestPostgres provides an empty database (a throwaway
/// postgres:16 container on CI, a fresh database on the dev cluster on the Mac), we apply the actual Titan
/// migrations to it once, and hand out fresh DbContexts bound to it. This is what lets an integration test exercise the invariants the EF InMemory provider cannot:
/// partial unique indexes, real transactions, and conditional-UPDATE concurrency.
///
/// Isolation between tests in the collection is by fresh Guids per test — rows persist in the shared
/// container but never collide, which keeps the harness fast (one container, migrated once) without
/// a truncate/respawn step. A test that needs a clean slate simply uses ids no other test uses.
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly TestPostgres _container = new();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = NewDbContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    /// A fresh context on the container — mirrors the scoped-per-request DbContext of the real app.
    /// Each repository call in a test should use its own context, not one shared long-lived instance,
    /// so the test reproduces the app's "new context per unit of work" reality (and its race windows).
    public TitanDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<TitanDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new TitanDbContext(options);
    }
}

/// The collection every Postgres-backed integration test joins via [Collection(PostgresCollection.Name)].
/// xUnit builds the fixture once and shares its container across the whole collection.
[CollectionDefinition(PostgresCollection.Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres-integration";
}

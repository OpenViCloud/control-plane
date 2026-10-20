using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;
using Titan.Infrastructure.Persistence;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Tests.Infrastructure;

// "One cluster per zone" and "one client identity" are DB guards, not application code. The InMemory provider does not
// enforce them, so this pins the model metadata the migration depends on.
public class ClusterSchemaTests
{
    private static IModel Model()
    {
        var options = new DbContextOptionsBuilder<TitanDbContext>()
            .UseInMemoryDatabase($"schema-{Guid.NewGuid()}")
            .Options;
        using var db = new TitanDbContext(options);
        return db.Model;
    }

    private static IEntityType Cluster() => Model().FindEntityType(typeof(ClusterAggregate))!;

    private static IEntityType Identity() => Model().FindEntityType(typeof(ProviderClientIdentity))!;

    [Fact]
    public void Cluster_IsMappedToTheClustersTable()
    {
        Assert.Equal("clusters", Cluster().GetTableName());
    }

    [Fact]
    public void OneClusterPerZone_UniqueIndexOnZoneId()
    {
        var index = Cluster().GetIndexes().SingleOrDefault(i =>
            i.IsUnique &&
            i.Properties.Select(p => p.Name).ToHashSet().SetEquals([nameof(ClusterAggregate.ZoneId)]));

        Assert.NotNull(index);
    }

    [Fact]
    public void ServerFingerprint_IsRequired()
    {
        var property = Cluster().FindProperty(nameof(ClusterAggregate.ServerFingerprint))!;

        Assert.False(property.IsNullable);
        Assert.Equal("server_fingerprint", property.GetColumnName());
    }

    // Null is "not set" (docs/features/cluster/design.md#cluster-provider-settings).
    [Fact]
    public void ProviderSettings_AreColumnsOfThisTable_AndAllNullable()
    {
        var settings = Cluster().FindComplexProperty(nameof(ClusterAggregate.ProviderSettings))!;
        var columns = settings.ComplexType.GetProperties()
            .ToDictionary(p => p.GetColumnName(), p => p.IsNullable);

        Assert.Equal(["ipv6_block", "ovn_uplink_network", "storage_pool"], columns.Keys.Order().ToArray());
        Assert.All(columns.Values, Assert.True);
    }

    // The fixed SingletonId is the only thing keeping Titan to one identity: a generated key would let a second row in.
    [Fact]
    public void ProviderClientIdentity_KeyIsTheFixedId_NeverGenerated()
    {
        var entity = Identity();
        var key = entity.FindPrimaryKey()!;

        Assert.Equal("provider_client_identities", entity.GetTableName());
        Assert.Equal([nameof(ProviderClientIdentity.Id)], key.Properties.Select(p => p.Name).ToArray());
        Assert.Equal(ValueGenerated.Never, entity.FindProperty(nameof(ProviderClientIdentity.Id))!.ValueGenerated);
    }

    [Fact]
    public void ProviderClientIdentity_KeyCipherAndCertificate_AreRequired()
    {
        var entity = Identity();

        Assert.False(entity.FindProperty(nameof(ProviderClientIdentity.KeyCipher))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(ProviderClientIdentity.CertificatePem))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(ProviderClientIdentity.Fingerprint))!.IsNullable);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Infrastructure.Persistence;

public class ClusterConfiguration : IEntityTypeConfiguration<ClusterAggregate>
{
    public void Configure(EntityTypeBuilder<ClusterAggregate> builder)
    {
        builder.ToTable("clusters");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.ZoneId).HasColumnName("zone_id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").IsRequired();
        builder.Property(c => c.Endpoint).HasColumnName("endpoint").IsRequired();
        builder.Property(c => c.ServerFingerprint).HasColumnName("server_fingerprint").HasMaxLength(64).IsRequired();
        // A value object, so ComplexProperty: columns in this table, nullable — null is "not set".
        builder.ComplexProperty(c => c.ProviderSettings, s =>
        {
            s.Property(x => x.OvnUplinkNetwork).HasColumnName("ovn_uplink_network");
            s.Property(x => x.StoragePool).HasColumnName("storage_pool");
            s.Property(x => x.Ipv6Block).HasColumnName("ipv6_block");
        });
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(c => c.ZoneId).IsUnique();
    }
}

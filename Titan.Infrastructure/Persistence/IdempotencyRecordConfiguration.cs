using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Titan.Infrastructure.Persistence;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(r => r.Key).HasColumnName("key").HasMaxLength(255).IsRequired();
        builder.Property(r => r.RequestHash).HasColumnName("request_hash").HasMaxLength(64).IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(r => r.ResponseStatusCode).HasColumnName("response_status_code");
        builder.Property(r => r.ResponseBody).HasColumnName("response_body");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.CompletedAt).HasColumnName("completed_at");

        builder.HasIndex(r => new { r.TenantId, r.Key }).IsUnique(); // race-safe dedup
        builder.HasIndex(r => r.CreatedAt);                          // for TTL cleanup
    }
}

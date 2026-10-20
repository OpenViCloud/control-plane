using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;

namespace Titan.Infrastructure.Persistence;

public class ProviderClientIdentityConfiguration : IEntityTypeConfiguration<ProviderClientIdentity>
{
    public void Configure(EntityTypeBuilder<ProviderClientIdentity> builder)
    {
        builder.ToTable("provider_client_identities");
        // The id is the aggregate's fixed SingletonId: the primary key is what keeps it to one row.
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.CertificatePem).HasColumnName("certificate_pem").IsRequired();
        builder.Property(i => i.KeyCipher).HasColumnName("key_cipher").IsRequired();
        builder.Property(i => i.Fingerprint).HasColumnName("fingerprint").HasMaxLength(64).IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}

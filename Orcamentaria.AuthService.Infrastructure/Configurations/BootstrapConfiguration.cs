using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Infrastructure.Configurations
{
    public class BootstrapConfiguration : IEntityTypeConfiguration<Bootstrap>
    {
        public void Configure(EntityTypeBuilder<Bootstrap> builder)
        {
            builder.ToTable("T_BOOTSTRAP");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .HasColumnType("BIGINT")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.ExpiresAt)
                .HasColumnName("EXPIRES_AT")
                .HasColumnType("DATETIME(6)");

            builder.Property(p => p.RevokedAt)
                .HasColumnName("REVOKED_AT")
                .HasColumnType("DATETIME(6)");

            builder.Property(p => p.Hash)
                .HasColumnName("HASH")
                .HasColumnType("VARCHAR(256)")
                .IsRequired();

            builder.Property(p => p.Active)
                .HasColumnName("ACTIVE")
                .HasColumnType("BIT")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(p => p.ServiceId)
                .HasColumnName("SERVICE_ID")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("DATETIME(6)")
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .HasColumnName("CREATED_BY")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Ignore(p => p.Service);
        }
    }
}

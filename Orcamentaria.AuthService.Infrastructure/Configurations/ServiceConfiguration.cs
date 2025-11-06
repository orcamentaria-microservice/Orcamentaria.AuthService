using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Infrastructure.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("T_SERVICE");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .HasColumnType("BIGINT")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Name)
                .HasColumnName("NAME")
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder.Property(p => p.ClientId)
                .HasColumnName("CLIENT_ID")
                .HasColumnType("VARCHAR(200)")
                .IsRequired();

            builder.Property(p => p.ClientSecret)
                .HasColumnName("CLIENT_SECRET")
                .HasColumnType("VARCHAR(200)")
                .IsRequired();

            builder.Property(p => p.Active)
                .HasColumnName("ACTIVE")
                .HasColumnType("BIT")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("DATETIME")
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .HasColumnName("CREATED_BY")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("UPDATED_AT")
                .HasColumnType("DATETIME")
                .IsRequired();

            builder.Property(p => p.UpdatedBy)
                .HasColumnName("UPDATED_BY")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Ignore(p => p.Bootstraps);

            builder.HasMany(p => p.Bootstraps)
                 .WithOne()
                 .HasForeignKey(p => p.ServiceId)
                 .HasConstraintName("fk_T_BOOTSTRAP_T_SERVICE")
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

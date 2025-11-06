using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Infrastructure.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("T_PERMISSION");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .HasColumnType("BIGINT")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Resource)
                .HasColumnName("RESOURCE")
                .HasColumnType("INT")
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("DESCRIPTION")
                .HasColumnType("VARCHAR(150)");

            builder.Property(p => p.Type)
                .HasColumnName("TYPE")
                .HasColumnType("INT")
                .IsRequired();

            builder.Property(p => p.IncrementalPermission)
                .HasColumnName("INCREMENTAL_PERMISSION")
                .HasColumnType("VARCHAR(50)");

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



        }
    }
}

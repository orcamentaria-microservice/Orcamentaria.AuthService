using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("T_USER");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .HasColumnType("BIGINT")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Name)
                .HasColumnName("NAME")
                .HasColumnType("VARCHAR(100)")
                .IsRequired();

            builder.Property(p => p.Email)
                .HasColumnName("EMAIL")
                .HasColumnType("VARCHAR(200)")
                .IsRequired();

            builder.Property(p => p.Password)
                .HasColumnName("PASSWORD")
                .HasColumnType("VARCHAR(300)")
                .IsRequired();

            builder.Property(p => p.CompanyId)
                .HasColumnName("COMPANY_ID")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Property(p => p.Active)
                .HasColumnName("ACTIVE")
                .HasColumnType("BIT")
                .HasDefaultValue(true)
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

            builder.Ignore(p => p.Permissions);

            builder
            .HasMany(u => u.Permissions)
            .WithMany(p => p.Users)
            .UsingEntity<Dictionary<string, object>>(
                "T_PERMISSION_USER",
                j => j
                    .HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey("PERMISSION_ID")
                    .HasConstraintName("fk_T_PERMISSION_USER_T_PERMISSION"),
                j => j
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("USER_ID")
                    .HasConstraintName("fk_T_PERMISSION_USER_T_USER"),
                j =>
                {
                    j.HasKey("USER_ID", "PERMISSION_ID");
                    j.ToTable("T_PERMISSION_USER");
                });
        }
    }
}

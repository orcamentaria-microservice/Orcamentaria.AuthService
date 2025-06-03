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
            builder.Property(p => p.Id).HasColumnName("ID");
            builder.Property(p => p.Name).HasColumnName("NAME");
            builder.Property(p => p.Email).HasColumnName("EMAIL");
            builder.Property(p => p.Password).HasColumnName("PASSWORD");
            builder.Property(p => p.CompanyId).HasColumnName("COMPANY_ID");
            builder.Property(p => p.Active).HasColumnName("ACTIVE");
            builder.Property(p => p.CreateAt).HasColumnName("CREATE_AT");
            builder.Property(p => p.UpdateAt).HasColumnName("UPDATE_AT");
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

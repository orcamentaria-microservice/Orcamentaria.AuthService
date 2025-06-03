using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Infrastructure.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("t_permission");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("ID");
            builder.Property(p => p.Resource).HasColumnName("RESOURCE");
            builder.Property(p => p.Description).HasColumnName("DESCRIPTION");
            builder.Property(p => p.Type).HasColumnName("TYPE");
            builder.Property(p => p.IncrementalPermission).HasColumnName("INCREMENTAL_PERMISSION");
            builder.Property(p => p.CreateAt).HasColumnName("CREATE_AT");
        }
    }
}

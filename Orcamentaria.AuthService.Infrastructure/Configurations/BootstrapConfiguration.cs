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
            builder.Property(p => p.Id).HasColumnName("ID");
            builder.Property(p => p.CreateAt).HasColumnName("CREATE_AT");
            builder.Property(p => p.ExpiresAt).HasColumnName("EXPIRES_AT");
            builder.Property(p => p.RevokedAt).HasColumnName("REVOKED_AT");
            builder.Property(p => p.Hash).HasColumnName("HASH");
            builder.Property(p => p.Active).HasColumnName("ACTIVE");
            builder.Property(p => p.ServiceId).HasColumnName("SERVICE_ID");
            builder.Ignore(p => p.Service);
        }
    }
}

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
            builder.Property(p => p.Id).HasColumnName("ID");
            builder.Property(p => p.Name).HasColumnName("NAME");
            builder.Property(p => p.ClientId).HasColumnName("CLIENT_ID");
            builder.Property(p => p.ClientSecret).HasColumnName("CLIENT_SECRET");
            builder.Property(p => p.Active).HasColumnName("ACTIVE");
        }
    }
}

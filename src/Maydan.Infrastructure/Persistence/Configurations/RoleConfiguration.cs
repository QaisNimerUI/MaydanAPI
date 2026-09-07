using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleNameAr)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(r => r.RoleNameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.RoleNameAr).IsUnique();
        builder.HasIndex(r => r.RoleNameEn).IsUnique();
    }
}

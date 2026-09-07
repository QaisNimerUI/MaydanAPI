using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.PermissionId);

        builder.Property(p => p.PermissionNameEn)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.PermissionNameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.PermissionNameEn).IsUnique();
        builder.HasIndex(p => p.PermissionNameAr).IsUnique();
    }
}

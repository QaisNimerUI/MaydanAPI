using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");

        builder.HasKey(g => g.GroupId);

        builder.Property(g => g.GroupNameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.GroupNameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(g => g.GroupNameEn).IsUnique();
        builder.HasIndex(g => g.GroupNameAr).IsUnique();
    }
}

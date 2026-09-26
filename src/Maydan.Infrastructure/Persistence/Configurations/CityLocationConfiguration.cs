using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class CityLocationConfiguration : IEntityTypeConfiguration<CityLocation>
{
    public void Configure(EntityTypeBuilder<CityLocation> builder)
    {
        builder.ToTable("CityLocations");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.ArabicName).IsRequired().HasMaxLength(200);
        builder.Property(cl => cl.EnglishName).IsRequired().HasMaxLength(200);

        builder.HasOne(cl => cl.City)
            .WithMany()
            .HasForeignKey(cl => cl.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

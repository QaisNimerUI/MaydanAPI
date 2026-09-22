using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class ProductionCompanyConfiguration : IEntityTypeConfiguration<ProductionCompany>
{
    public void Configure(EntityTypeBuilder<ProductionCompany> builder)
    {
        builder.ToTable("ProductionCompanies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ArabicName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.EnglishName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.ContactPhone).HasMaxLength(30);
        builder.Property(p => p.ContactEmail).HasMaxLength(200);

        builder.Property(p => p.IsSelfRegistered).HasDefaultValue(false);

        // Entity onboarding Stage 1 (2026-09-22): added for the public production-company
        // self-registration endpoint. RegistrationNumber unique per confirmed product decision —
        // it identifies one real-world legal entity, so two self-registrations can't share it.
        builder.Property(p => p.RegistrationNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.RegistrationNumber).IsUnique();

        builder.HasOne(p => p.City)
            .WithMany()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

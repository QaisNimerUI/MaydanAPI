using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
{
    public void Configure(EntityTypeBuilder<Worker> builder)
    {
        builder.ToTable("Workers");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(w => w.MiddleName).HasMaxLength(100);
        builder.Property(w => w.LastName).IsRequired().HasMaxLength(100);

        // Sized for ciphertext, not the raw national ID. No unique index: the encryption is
        // non-deterministic, so uniqueness is enforced via CivilIdHash (blind index) instead.
        builder.Property(w => w.CivilId).IsRequired().HasMaxLength(256);

        // HMAC-SHA256 hex digest is always 64 chars.
        builder.Property(w => w.CivilIdHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(w => w.CivilIdHash).IsUnique();

        builder.Property(w => w.PhoneNumber).HasMaxLength(30);

        builder.Property(w => w.QrCode).IsRequired().HasMaxLength(200);
        builder.HasIndex(w => w.QrCode).IsUnique();

        builder.Property(w => w.IsActive).HasDefaultValue(true);

        builder.HasOne(w => w.Association)
            .WithMany(a => a.Workers)
            .HasForeignKey(w => w.AssociationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

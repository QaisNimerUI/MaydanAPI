using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SmtpHost).IsRequired().HasMaxLength(256);
        builder.Property(c => c.SmtpUsername).IsRequired().HasMaxLength(256);

        // Sized for ciphertext (ASP.NET Core Data Protection payloads run well past the raw
        // password length) — see ISecretProtector's own comment.
        builder.Property(c => c.SmtpPasswordProtected).IsRequired().HasMaxLength(2000);

        builder.Property(c => c.SenderEmail).IsRequired().HasMaxLength(256);
        builder.Property(c => c.SenderDisplayName).IsRequired().HasMaxLength(200);
    }
}

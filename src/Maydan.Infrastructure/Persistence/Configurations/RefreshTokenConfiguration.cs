using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.ExpiresAtUtc).IsRequired();
        builder.Property(t => t.IsRevoked).IsRequired().HasDefaultValue(false);

        // ReplacedByTokenId is deliberately NOT mapped as a self-referencing FK/navigation — see
        // RefreshToken.cs's own comment. It's a plain nullable int column, same soft-pointer
        // treatment this codebase already gives User.EntityId.

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

// Forgot-password recovery (MAYD-128/129/130, 2026-09-23): a real FK to User (unlike the
// EntityType/EntityId soft-FK pattern used for Association/ProductionCompany/etc. — this always
// points at exactly one table). TokenHash stores the SAME salted-hash format IPasswordHasher
// already produces for passwords (PasswordHasher.cs) — the raw token is never persisted, only
// emailed to the user once. Because that hash is salted, looking a token up means fetching
// candidate rows and calling IPasswordHasher.VerifyPassword() against each rather than a direct
// hash-equality query — see AuthService.ResetPasswordWithTokenAsync's own comment on why that's
// fine at this table's expected scale.
public class PasswordResetToken : SharedEntities
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAtUtc { get; set; }
}

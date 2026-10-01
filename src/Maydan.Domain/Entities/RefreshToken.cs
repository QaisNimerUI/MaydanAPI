using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

// Real 3-week session persistence + "remember me" (MAYD-131/132, 2026-09-23). Same hashed-token
// convention as PasswordResetToken (see that entity's own comment) — TokenHash is a salted hash of
// the raw value actually handed to the client, never the raw value itself; lookup means scanning
// candidates and calling IPasswordHasher.VerifyPassword() against each, same trade-off.
//
// ReplacedByTokenId is a soft pointer (no EF navigation/FK), same "soft FK" pattern this codebase
// already uses for User.EntityId — it's only ever read back by Id after a rotation, never
// navigated/joined, so a real self-referencing FK relationship would be unnecessary ceremony. It's
// set ONLY when this token was exchanged via normal rotation (RefreshTokenAsync); a revoked token
// with this left null was invalidated directly instead (logout, or a password change revoking every
// session at once). That distinction is what lets a reused, already-rotated-away token be told apart
// from an ordinary "this session already ended" case — see AuthService.RefreshTokenAsync's own
// comment on why that distinction matters.
public class RefreshToken : SharedEntities
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public int? ReplacedByTokenId { get; set; }
}

using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IRefreshTokenRepository
{
    // Returns every non-deleted token regardless of revoked/expired status — RefreshTokenAsync needs
    // the matched row's own IsRevoked/ReplacedByTokenId/ExpiresAtUtc to tell an ordinary expiry apart
    // from a reused-already-rotated-token (theft) signal, not just a filtered yes/no. Same shape as
    // IPasswordResetTokenRepository.GetAllAsync for the same reason.
    Task<List<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
}

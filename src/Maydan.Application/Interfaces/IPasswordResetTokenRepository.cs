using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IPasswordResetTokenRepository
{
    // Returns every non-deleted token regardless of used/expired status — ResetPasswordWithTokenAsync
    // needs the matched row's own IsUsed/ExpiresAtUtc to report a specific, distinct error
    // ("already used" vs "expired" vs "invalid"), not just a filtered yes/no.
    Task<List<PasswordResetToken>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
}

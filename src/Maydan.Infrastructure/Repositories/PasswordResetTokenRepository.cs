using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly MaydanDbContext _context;

    public PasswordResetTokenRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<List<PasswordResetToken>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.PasswordResetTokens.ToListAsync(cancellationToken);

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default) =>
        await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
}

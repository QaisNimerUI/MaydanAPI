using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly MaydanDbContext _context;

    public RefreshTokenRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<List<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.RefreshTokens.ToListAsync(cancellationToken);

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.AddAsync(token, cancellationToken);
}

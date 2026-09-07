using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class AssociationRepository : IAssociationRepository
{
    private readonly MaydanDbContext _context;

    public AssociationRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Association?> GetByIdAsync(int associationId, CancellationToken cancellationToken = default) =>
        _context.Associations.FirstOrDefaultAsync(a => a.Id == associationId, cancellationToken);

    public Task<List<Association>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Associations.ToListAsync(cancellationToken);

    public async Task AddAsync(Association association, CancellationToken cancellationToken = default) =>
        await _context.Associations.AddAsync(association, cancellationToken);

    public void Remove(Association association) => _context.Associations.Remove(association);
}

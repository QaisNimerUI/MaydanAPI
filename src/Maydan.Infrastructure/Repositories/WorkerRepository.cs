using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class WorkerRepository : IWorkerRepository
{
    private readonly MaydanDbContext _context;

    public WorkerRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Worker?> GetByIdAsync(int workerId, CancellationToken cancellationToken = default) =>
        _context.Workers.FirstOrDefaultAsync(w => w.Id == workerId, cancellationToken);

    public Task<Worker?> GetByCivilIdHashAsync(string civilIdHash, CancellationToken cancellationToken = default) =>
        _context.Workers.FirstOrDefaultAsync(w => w.CivilIdHash == civilIdHash, cancellationToken);

    public Task<List<Worker>> GetByAssociationIdAsync(int associationId, CancellationToken cancellationToken = default) =>
        _context.Workers.Where(w => w.AssociationId == associationId).ToListAsync(cancellationToken);

    public async Task AddAsync(Worker worker, CancellationToken cancellationToken = default) =>
        await _context.Workers.AddAsync(worker, cancellationToken);

    public void Remove(Worker worker) => _context.Workers.Remove(worker);
}

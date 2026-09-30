using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly MaydanDbContext _context;

    public ServiceRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Set<Service>().ToListAsync(cancellationToken);

    public Task<List<Service>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        _context.Set<Service>().Where(s => s.IsActive).ToListAsync(cancellationToken);

    public Task<Service?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Set<Service>().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Service?> FindByNameEnAsync(string nameEn, CancellationToken cancellationToken = default) =>
        _context.Set<Service>().FirstOrDefaultAsync(s => s.NameEn == nameEn, cancellationToken);

    public Task<Service?> FindByNameArAsync(string nameAr, CancellationToken cancellationToken = default) =>
        _context.Set<Service>().FirstOrDefaultAsync(s => s.NameAr == nameAr, cancellationToken);

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default) =>
        await _context.Set<Service>().AddAsync(service, cancellationToken);

    public void Update(Service service) => _context.Set<Service>().Update(service);

    public void Remove(Service service) => _context.Set<Service>().Remove(service);
}

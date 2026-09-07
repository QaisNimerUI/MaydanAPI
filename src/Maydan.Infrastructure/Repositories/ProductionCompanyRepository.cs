using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class ProductionCompanyRepository : IProductionCompanyRepository
{
    private readonly MaydanDbContext _context;

    public ProductionCompanyRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<ProductionCompany?> GetByIdAsync(int productionCompanyId, CancellationToken cancellationToken = default) =>
        _context.ProductionCompanies.FirstOrDefaultAsync(p => p.Id == productionCompanyId, cancellationToken);

    public Task<List<ProductionCompany>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.ProductionCompanies.ToListAsync(cancellationToken);

    public async Task AddAsync(ProductionCompany productionCompany, CancellationToken cancellationToken = default) =>
        await _context.ProductionCompanies.AddAsync(productionCompany, cancellationToken);

    public void Remove(ProductionCompany productionCompany) => _context.ProductionCompanies.Remove(productionCompany);
}

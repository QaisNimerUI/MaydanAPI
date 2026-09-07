using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IProductionCompanyRepository
{
    Task<ProductionCompany?> GetByIdAsync(int productionCompanyId, CancellationToken cancellationToken = default);
    Task<List<ProductionCompany>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProductionCompany productionCompany, CancellationToken cancellationToken = default);
    void Remove(ProductionCompany productionCompany);
}

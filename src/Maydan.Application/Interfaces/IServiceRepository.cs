using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IServiceRepository
{
    Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Service>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Service?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Service?> FindByNameEnAsync(string nameEn, CancellationToken cancellationToken = default);
    Task<Service?> FindByNameArAsync(string nameAr, CancellationToken cancellationToken = default);
    Task AddAsync(Service service, CancellationToken cancellationToken = default);
    void Update(Service service);
    void Remove(Service service);
}

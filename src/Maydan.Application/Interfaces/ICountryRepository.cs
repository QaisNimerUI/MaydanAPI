using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface ICountryRepository
{
    Task<Country?> GetByIdAsync(int countryId, CancellationToken cancellationToken = default);
    Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Country country, CancellationToken cancellationToken = default);
    void Remove(Country country);
}

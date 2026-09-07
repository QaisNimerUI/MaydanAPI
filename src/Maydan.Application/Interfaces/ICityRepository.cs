using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface ICityRepository
{
    Task<City?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default);
    Task<List<City>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default);
    Task AddAsync(City city, CancellationToken cancellationToken = default);
    void Remove(City city);
}

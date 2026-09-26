using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class CityLocationRepository : ICityLocationRepository
{
    private readonly MaydanDbContext _context;

    public CityLocationRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<CityLocation?> GetByIdAsync(int cityLocationId, CancellationToken cancellationToken = default) =>
        _context.CityLocations.FirstOrDefaultAsync(cl => cl.Id == cityLocationId, cancellationToken);

    public Task<CityLocation?> GetByIdIncludingDeletedAsync(int cityLocationId, CancellationToken cancellationToken = default) =>
        _context.CityLocations.IgnoreQueryFilters().FirstOrDefaultAsync(cl => cl.Id == cityLocationId, cancellationToken);

    public Task<List<CityLocation>> GetByCityIdAsync(int cityId, CancellationToken cancellationToken = default) =>
        _context.CityLocations.IgnoreQueryFilters().AsNoTracking().Where(cl => cl.CityId == cityId).OrderBy(cl => cl.EnglishName).ToListAsync(cancellationToken);

    public async Task AddAsync(CityLocation cityLocation, CancellationToken cancellationToken = default) =>
        await _context.CityLocations.AddAsync(cityLocation, cancellationToken);

    public void Remove(CityLocation cityLocation) => _context.CityLocations.Remove(cityLocation);
}

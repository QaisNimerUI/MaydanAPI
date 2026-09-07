using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly MaydanDbContext _context;

    public CountryRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Country?> GetByIdAsync(int countryId, CancellationToken cancellationToken = default) =>
        _context.Countries.FirstOrDefaultAsync(c => c.Id == countryId, cancellationToken);

    public Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Countries.ToListAsync(cancellationToken);

    public async Task AddAsync(Country country, CancellationToken cancellationToken = default) =>
        await _context.Countries.AddAsync(country, cancellationToken);

    public void Remove(Country country) => _context.Countries.Remove(country);
}

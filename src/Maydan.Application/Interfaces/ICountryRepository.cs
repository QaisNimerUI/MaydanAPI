using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface ICountryRepository
{
    // Active-only — used to validate a real, non-deleted Country FK (e.g. before creating a City
    // under it). Excludes soft-deleted rows via MaydanDbContext's global query filter.
    Task<Country?> GetByIdAsync(int countryId, CancellationToken cancellationToken = default);

    // Bypasses the soft-delete filter — needed by RestoreAsync, which must load a row the normal
    // filter would otherwise hide precisely because it's deleted (same pattern as
    // IProjectRepository.GetByIdIncludingDeletedAsync).
    Task<Country?> GetByIdIncludingDeletedAsync(int countryId, CancellationToken cancellationToken = default);

    // MAYD-locations Phase 1: the admin Locations page (location-management.component.html) shows
    // active AND soft-deleted countries in the SAME list — a deleted row renders with a "Deleted"
    // pill and a Restore button instead of being hidden, so this deliberately bypasses the global
    // soft-delete filter rather than returning active-only rows. This is a different shape than
    // ProjectRepository.GetAllAsync's isDeleted-toggle (two separate views) because
    // CountriesService.getAll() on the frontend takes no such toggle at all — there is only one
    // list endpoint, and it must carry both states for the admin UI to work.
    Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Country country, CancellationToken cancellationToken = default);
    void Remove(Country country);
}

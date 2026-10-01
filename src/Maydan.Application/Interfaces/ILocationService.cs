using Maydan.Application.DTOs.Locations;

namespace Maydan.Application.Interfaces;

// Country and City combined into one service, the same "one cohesive service per feature area"
// shape IUserManagementService already established for Users/Groups/Permissions together, rather
// than fragmenting into ICountryService + ICityService — City is FK'd to Country, and
// workforcment's own frontend treats them as a single "locations" feature (one models file, one
// location-management page driving both via a mode switch).
public interface ILocationService
{
    Task<List<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<CountryDto> GetCountryByIdAsync(int countryId, CancellationToken cancellationToken = default);
    Task<CountryDto> CreateCountryAsync(CreateCountryDto dto, int currentUserId, CancellationToken cancellationToken = default);
    Task<CountryDto> UpdateCountryAsync(UpdateCountryDto dto, int currentUserId, CancellationToken cancellationToken = default);
    Task DeleteCountryAsync(int countryId, int currentUserId, CancellationToken cancellationToken = default);
    Task<CountryDto> RestoreCountryAsync(int countryId, int currentUserId, CancellationToken cancellationToken = default);

    Task<List<CityDto>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken = default);
    Task<CityDto> CreateCityAsync(CreateCityDto dto, int currentUserId, CancellationToken cancellationToken = default);
    Task<CityDto> UpdateCityAsync(int cityId, UpdateCityDto dto, int currentUserId, CancellationToken cancellationToken = default);
    Task DeleteCityAsync(int cityId, int currentUserId, CancellationToken cancellationToken = default);
    Task<CityDto> RestoreCityAsync(int cityId, int currentUserId, CancellationToken cancellationToken = default);

    // MAYD-51 Phase 2d: CityLocations, the third and last tier of this same feature — see
    // LocationService's own CityLocation method comments for the rest of the story.
    Task<List<CityLocationDto>> GetCityLocationsByCityAsync(int cityId, CancellationToken cancellationToken = default);
    Task<CityLocationDto> CreateCityLocationAsync(CreateCityLocationDto dto, int currentUserId, CancellationToken cancellationToken = default);
    Task<CityLocationDto> UpdateCityLocationAsync(UpdateCityLocationDto dto, int currentUserId, CancellationToken cancellationToken = default);
    Task DeleteCityLocationAsync(int cityLocationId, int currentUserId, CancellationToken cancellationToken = default);
    Task<CityLocationDto> RestoreCityLocationAsync(int cityLocationId, int currentUserId, CancellationToken cancellationToken = default);
}

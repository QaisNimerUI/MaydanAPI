using Maydan.Application.DTOs.Locations;
using Maydan.Application.Interfaces;
using Maydan.Application.Services;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Tests.Services;

// Associations/Country/City backend gap, Phase 1 (2026-09-25) — LocationService is the first real
// consumer of ICountryRepository/ICityRepository; these tests cover the CRUD+restore surface added
// alongside it, same hand-rolled-fake pattern as ProjectServiceTests/UserManagementServiceTests
// (no mocking library in this test project).
public class LocationServiceTests
{
    private static User NewUser(int userId, bool isActive = true) => new()
    {
        UserId = userId,
        FirstNameEn = $"First{userId}",
        LastNameEn = $"Last{userId}",
        FirstNameAr = $"اول{userId}",
        LastNameAr = $"اخير{userId}",
        RoleId = 1,
        EntityType = EntityType.ProductionCompany,
        EntityId = 1,
        IsActive = isActive
    };

    // ---------------------------------------------------------------------
    // Country CRUD + restore
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CreateCountryAsync_ValidRequest_Succeeds()
    {
        var currentUser = NewUser(1);
        var (service, countryRepository, _) = BuildService(users: [currentUser]);

        var result = await service.CreateCountryAsync(
            new CreateCountryDto { CountryEnglishName = "Jordan", CountryArabicName = "الأردن" },
            currentUser.UserId);

        Assert.Equal("Jordan", result.CountryEnglishName);
        Assert.Equal("الأردن", result.CountryArabicName);
        Assert.False(result.IsDeleted);
        Assert.NotNull(countryRepository.AddedCountry);
        Assert.Equal(currentUser.UserId, countryRepository.AddedCountry!.CreatedBy);
    }

    [Fact]
    public async Task CreateCountryAsync_MissingEnglishName_ThrowsInvalidOperationException()
    {
        var currentUser = NewUser(1);
        var (service, _, _) = BuildService(users: [currentUser]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateCountryAsync(
                new CreateCountryDto { CountryEnglishName = "  ", CountryArabicName = "الأردن" },
                currentUser.UserId));
    }

    [Fact]
    public async Task GetCountriesAsync_ReturnsActiveAndDeletedTogether()
    {
        var currentUser = NewUser(1);
        var active = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true, IsDeleted = false };
        var deleted = new Country { Id = 2, EnglishName = "Egypt", ArabicName = "مصر", IsActive = true, IsDeleted = true };
        var (service, _, _) = BuildService(users: [currentUser], countries: [active, deleted]);

        var result = await service.GetCountriesAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == 1 && !c.IsDeleted);
        Assert.Contains(result, c => c.Id == 2 && c.IsDeleted);
    }

    [Fact]
    public async Task DeleteCountryAsync_WithActiveCities_ThrowsInvalidOperationException()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };
        var city = new City { Id = 1, CountryId = 1, EnglishName = "Amman", ArabicName = "عمان", IsActive = true };
        var (service, _, _) = BuildService(users: [currentUser], countries: [country], cities: [city]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteCountryAsync(country.Id, currentUser.UserId));
    }

    [Fact]
    public async Task DeleteCountryAsync_WithNoActiveCities_SoftDeletes()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };
        var deletedCity = new City { Id = 1, CountryId = 1, EnglishName = "Amman", ArabicName = "عمان", IsActive = true, IsDeleted = true };
        var (service, countryRepository, _) = BuildService(users: [currentUser], countries: [country], cities: [deletedCity]);

        await service.DeleteCountryAsync(country.Id, currentUser.UserId);

        Assert.NotNull(countryRepository.RemovedCountry);
        Assert.True(country.IsDeleted);
    }

    [Fact]
    public async Task RestoreCountryAsync_DeletedCountry_ClearsIsDeleted()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true, IsDeleted = true, DeletedAt = DateTime.UtcNow };
        var (service, _, _) = BuildService(users: [currentUser], countries: [country]);

        var result = await service.RestoreCountryAsync(country.Id, currentUser.UserId);

        Assert.False(result.IsDeleted);
        Assert.False(country.IsDeleted);
        Assert.Null(country.DeletedAt);
    }

    [Fact]
    public async Task RestoreCountryAsync_NotDeleted_ThrowsInvalidOperationException()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true, IsDeleted = false };
        var (service, _, _) = BuildService(users: [currentUser], countries: [country]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RestoreCountryAsync(country.Id, currentUser.UserId));
    }

    [Fact]
    public async Task RestoreCountryAsync_UnknownId_ThrowsKeyNotFoundException()
    {
        var currentUser = NewUser(1);
        var (service, _, _) = BuildService(users: [currentUser]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.RestoreCountryAsync(999, currentUser.UserId));
    }

    // ---------------------------------------------------------------------
    // City CRUD + restore
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CreateCityAsync_ValidRequest_Succeeds()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };
        var (service, _, cityRepository) = BuildService(users: [currentUser], countries: [country]);

        var result = await service.CreateCityAsync(
            new CreateCityDto { CityEnglishName = "Amman", CityArabicName = "عمان", CountryId = country.Id },
            currentUser.UserId);

        Assert.Equal("Amman", result.CityEnglishName);
        Assert.Equal(country.Id, result.CountryId);
        Assert.NotNull(cityRepository.AddedCity);
        Assert.Equal(currentUser.UserId, cityRepository.AddedCity!.CreatedBy);
    }

    [Fact]
    public async Task CreateCityAsync_UnknownCountry_ThrowsKeyNotFoundException()
    {
        var currentUser = NewUser(1);
        var (service, _, _) = BuildService(users: [currentUser]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateCityAsync(
                new CreateCityDto { CityEnglishName = "Amman", CityArabicName = "عمان", CountryId = 999 },
                currentUser.UserId));
    }

    [Fact]
    public async Task CreateCityAsync_DeletedCountry_ThrowsKeyNotFoundException()
    {
        // A soft-deleted Country must reject new Cities the same way an unknown one does — Countries
        // repository's GetByIdAsync stays filtered (see its own interface comment).
        var currentUser = NewUser(1);
        var deletedCountry = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true, IsDeleted = true };
        var (service, _, _) = BuildService(users: [currentUser], countries: [deletedCountry]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateCityAsync(
                new CreateCityDto { CityEnglishName = "Amman", CityArabicName = "عمان", CountryId = deletedCountry.Id },
                currentUser.UserId));
    }

    [Fact]
    public async Task GetCitiesByCountryAsync_ReturnsActiveAndDeletedTogether()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };
        var active = new City { Id = 1, CountryId = 1, EnglishName = "Amman", ArabicName = "عمان", IsActive = true, IsDeleted = false };
        var deleted = new City { Id = 2, CountryId = 1, EnglishName = "Aqaba", ArabicName = "العقبة", IsActive = true, IsDeleted = true };
        var (service, _, _) = BuildService(users: [currentUser], countries: [country], cities: [active, deleted]);

        var result = await service.GetCitiesByCountryAsync(country.Id);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == 1 && !c.IsDeleted);
        Assert.Contains(result, c => c.Id == 2 && c.IsDeleted);
    }

    [Fact]
    public async Task RestoreCityAsync_DeletedCity_ClearsIsDeleted()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };
        var city = new City { Id = 1, CountryId = 1, EnglishName = "Amman", ArabicName = "عمان", IsActive = true, IsDeleted = true, DeletedAt = DateTime.UtcNow };
        var (service, _, _) = BuildService(users: [currentUser], countries: [country], cities: [city]);

        var result = await service.RestoreCityAsync(city.Id, currentUser.UserId);

        Assert.False(result.IsDeleted);
        Assert.False(city.IsDeleted);
        Assert.Null(city.DeletedAt);
    }

    [Fact]
    public async Task DeleteCityAsync_ExistingCity_SoftDeletes()
    {
        var currentUser = NewUser(1);
        var country = new Country { Id = 1, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };
        var city = new City { Id = 1, CountryId = 1, EnglishName = "Amman", ArabicName = "عمان", IsActive = true };
        var (service, _, cityRepository) = BuildService(users: [currentUser], countries: [country], cities: [city]);

        await service.DeleteCityAsync(city.Id, currentUser.UserId);

        Assert.NotNull(cityRepository.RemovedCity);
        Assert.True(city.IsDeleted);
    }

    private static (LocationService Service, FakeCountryRepository CountryRepository, FakeCityRepository CityRepository) BuildService(
        User[] users,
        Country[]? countries = null,
        City[]? cities = null)
    {
        var usersById = users.ToDictionary(u => u.UserId);
        var userRepository = new FakeUserRepository(usersById);
        var countryRepository = new FakeCountryRepository(countries ?? []);
        var cityRepository = new FakeCityRepository(cities ?? []);

        var unitOfWork = new FakeUnitOfWork(userRepository, countryRepository, cityRepository);
        return (new LocationService(unitOfWork), countryRepository, cityRepository);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<int, User> _usersById;

        public FakeUserRepository(Dictionary<int, User> usersById) => _usersById = usersById;

        public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usersById.GetValueOrDefault(userId));

        public Task<User?> GetByEmailWithAccessAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByEntityAsync(EntityType entityType, int entityId, string? search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetDeletedByEntityAsync(EntityType entityType, int entityId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<User> Users, int TotalCount)> GetPagedByEntityAsync(EntityType entityType, int entityId, string? search, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsReadOnlyAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByIdsInEntityAsync(IEnumerable<int> userIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(User user) => throw new NotSupportedException();
    }

    // Remove() simulates MaydanDbContext.SaveChangesAsync's global soft-delete interception
    // directly, same as FakeProjectRepository.Remove — there's no real change-tracking pipeline in
    // this fake to intercept it for us.
    private sealed class FakeCountryRepository : ICountryRepository
    {
        private readonly Dictionary<int, Country> _countriesById;

        public FakeCountryRepository(IEnumerable<Country> countries) => _countriesById = countries.ToDictionary(c => c.Id);

        public Country? AddedCountry { get; private set; }
        public Country? RemovedCountry { get; private set; }

        public Task<Country?> GetByIdAsync(int countryId, CancellationToken cancellationToken = default)
        {
            var country = _countriesById.GetValueOrDefault(countryId);
            return Task.FromResult(country is { IsDeleted: false } ? country : null);
        }

        public Task<Country?> GetByIdIncludingDeletedAsync(int countryId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_countriesById.GetValueOrDefault(countryId));

        public Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_countriesById.Values.ToList());

        public Task AddAsync(Country country, CancellationToken cancellationToken = default)
        {
            country.Id = country.Id == 0 ? _countriesById.Count + 100 : country.Id;
            AddedCountry = country;
            _countriesById[country.Id] = country;
            return Task.CompletedTask;
        }

        public void Remove(Country country)
        {
            country.IsDeleted = true;
            country.DeletedAt = DateTime.UtcNow;
            RemovedCountry = country;
        }
    }

    private sealed class FakeCityRepository : ICityRepository
    {
        private readonly Dictionary<int, City> _citiesById;

        public FakeCityRepository(IEnumerable<City> cities) => _citiesById = cities.ToDictionary(c => c.Id);

        public City? AddedCity { get; private set; }
        public City? RemovedCity { get; private set; }

        public Task<City?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default)
        {
            var city = _citiesById.GetValueOrDefault(cityId);
            return Task.FromResult(city is { IsDeleted: false } ? city : null);
        }

        public Task<City?> GetByIdIncludingDeletedAsync(int cityId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_citiesById.GetValueOrDefault(cityId));

        public Task<List<City>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_citiesById.Values.Where(c => c.CountryId == countryId).ToList());

        public Task AddAsync(City city, CancellationToken cancellationToken = default)
        {
            city.Id = city.Id == 0 ? _citiesById.Count + 100 : city.Id;
            AddedCity = city;
            _citiesById[city.Id] = city;
            return Task.CompletedTask;
        }

        public void Remove(City city)
        {
            city.IsDeleted = true;
            city.DeletedAt = DateTime.UtcNow;
            RemovedCity = city;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IUserRepository users, ICountryRepository countries, ICityRepository cities)
        {
            Users = users;
            Countries = countries;
            Cities = cities;
        }

        public IUserRepository Users { get; }
        public ICountryRepository Countries { get; }
        public ICityRepository Cities { get; }
        public IRoleRepository Roles => throw new NotSupportedException();
        public IPermissionRepository Permissions => throw new NotSupportedException();
        public IGroupRepository Groups => throw new NotSupportedException();
        public IProjectTypeRepository ProjectTypes => throw new NotSupportedException();
        public IAssociationRepository Associations => throw new NotSupportedException();
        public IProductionCompanyRepository ProductionCompanies => throw new NotSupportedException();
        public IProjectRepository Projects => throw new NotSupportedException();
        public IWorkerRepository Workers => throw new NotSupportedException();
        public IPasswordResetTokenRepository PasswordResetTokens => throw new NotSupportedException();
        public IRefreshTokenRepository RefreshTokens => throw new NotSupportedException();
        public ISystemConfigurationRepository SystemConfigurations => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default) => operation();
    }
}

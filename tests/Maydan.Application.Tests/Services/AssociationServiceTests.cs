using Maydan.Application.DTOs.Associations;
using Maydan.Application.Interfaces;
using Maydan.Application.Services;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Tests.Services;

// Association Management, Phase 2a (MAYD-4, MAYD-40..54) — AssociationService is the first real
// consumer of IAssociationRepository's new members; these tests cover the CRUD+restore surface and
// the fine-grained Associations permission catalog (ViewAssociations/CreateAssociations/
// EditAssociations/DeleteAssociations/ManageAssociations — five distinct ids, not just two) this
// pass wires up. Same hand-rolled-fake-per-file pattern as LocationServiceTests/
// UserManagementServiceUpdateUserStatusTests (no mocking library in this test project).
public class AssociationServiceTests
{
    private const int ViewAssociations = 9;
    private const int CreateAssociations = 10;
    private const int EditAssociations = 11;
    private const int DeleteAssociations = 12;
    private const int ManageAssociations = 13;

    private static Role BuildRole(int roleId, string nameEn, params int[] permissionIds)
    {
        var role = new Role { RoleId = roleId, RoleNameEn = nameEn, RoleNameAr = nameEn };
        foreach (var permissionId in permissionIds)
        {
            var permission = new Permission { PermissionId = permissionId, PermissionNameEn = $"Permission{permissionId}", PermissionNameAr = $"Permission{permissionId}", Module = "Associations", IsActive = true };
            role.RolePermissions.Add(new RolePermission { RoleId = roleId, Role = role, PermissionId = permissionId, Permission = permission, IsActive = true });
        }

        return role;
    }

    private static User BuildUser(int userId, Role role, bool isActive = true) => new()
    {
        UserId = userId,
        FirstNameEn = $"First{userId}",
        LastNameEn = $"Last{userId}",
        FirstNameAr = $"اول{userId}",
        LastNameAr = $"اخير{userId}",
        RoleId = role.RoleId,
        Role = role,
        EntityType = EntityType.BaytAlUrdon,
        EntityId = 1,
        IsActive = isActive
    };

    private static Country BuildCountry(int id) => new() { Id = id, EnglishName = "Jordan", ArabicName = "الأردن", IsActive = true };

    private static City BuildCity(int id, Country country) => new() { Id = id, CountryId = country.Id, Country = country, EnglishName = "Amman", ArabicName = "عمان", IsActive = true };

    private static Association BuildAssociation(int id, City city, bool isDeleted = false) => new()
    {
        Id = id,
        EnglishName = $"Association {id}",
        ArabicName = $"جمعية {id}",
        CityId = city.Id,
        City = city,
        IsDeleted = isDeleted,
        IsActive = true
    };

    [Fact]
    public async Task GetAllAsync_CallerWithViewAssociations_ReturnsMappedDtos()
    {
        var caller = BuildUser(1, BuildRole(1, "Bayt-AlUrdon", ViewAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city);

        var service = BuildService(caller, associations: [association]);

        var result = await service.GetAllAsync(caller.UserId);

        var dto = Assert.Single(result);
        Assert.Equal("Association 10", dto.EnglishName);
        Assert.Equal(country.Id, dto.CountryId);
        Assert.Equal("Jordan", dto.CountryEnglishName);
        Assert.Equal("Amman", dto.CityEnglishName);
        Assert.False(dto.IsDeleted);
    }

    [Fact]
    public async Task GetAllAsync_CallerWithoutViewOrManage_ThrowsUnauthorizedAccessException()
    {
        var caller = BuildUser(1, BuildRole(1, "NoAccess"));
        var service = BuildService(caller);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(caller.UserId));
    }

    [Fact]
    public async Task GetAllAsync_CallerWithManageAssociationsOnly_Succeeds()
    {
        // ManageAssociations acts as a full View/Create/Edit/Delete substitute — confirmed via both
        // the real seed data (RolePermissionSeedConfiguration.cs's own comment) and the real
        // frontend's own permission gates (associations-list.component.ts).
        var caller = BuildUser(1, BuildRole(1, "Manager", ManageAssociations));
        var service = BuildService(caller);

        var result = await service.GetAllAsync(caller.UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_CallerWithCreateAssociations_Succeeds()
    {
        var caller = BuildUser(1, BuildRole(1, "Bayt-AlUrdon", CreateAssociations, ViewAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(5, country);
        var service = BuildService(caller, cities: [city]);

        var dto = new CreateAssociationDto { EnglishName = "New Assoc", ArabicName = "جمعية جديدة", CityId = city.Id, Latitude = "31.953000", Longitude = "35.910500" };

        var result = await service.CreateAsync(caller.UserId, dto);

        Assert.Equal("New Assoc", result.EnglishName);
        Assert.Equal(city.Id, result.CityId);
        Assert.Equal("31.953", result.Latitude);
    }

    [Fact]
    public async Task CreateAsync_CallerWithoutCreatePermission_ThrowsUnauthorizedAccessException()
    {
        // The real open question this pass flags rather than guesses at: a caller holding ONLY
        // ViewAssociations (read) must not be able to create — proves Create is independently gated,
        // not implied by View.
        var caller = BuildUser(1, BuildRole(1, "ViewOnly", ViewAssociations));
        var service = BuildService(caller);

        var dto = new CreateAssociationDto { EnglishName = "X", ArabicName = "س", CityId = 1 };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(caller.UserId, dto));
    }

    [Fact]
    public async Task CreateAsync_UnknownCity_ThrowsKeyNotFoundException()
    {
        var caller = BuildUser(1, BuildRole(1, "Bayt-AlUrdon", CreateAssociations));
        var service = BuildService(caller);

        var dto = new CreateAssociationDto { EnglishName = "X", ArabicName = "س", CityId = 999 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(caller.UserId, dto));
    }

    [Fact]
    public async Task UpdateAsync_CallerWithCreateButNotEditAssociations_ThrowsUnauthorizedAccessException()
    {
        // Direct proof that Create and Edit are genuinely separate gates in this implementation —
        // holding CreateAssociations alone does not imply EditAssociations.
        var caller = BuildUser(1, BuildRole(1, "CreateOnly", CreateAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city);
        var service = BuildService(caller, associations: [association], cities: [city]);

        var dto = new UpdateAssociationDto { Id = 10, EnglishName = "Updated", ArabicName = "محدثة", CityId = city.Id };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(caller.UserId, dto));
    }

    [Fact]
    public async Task UpdateAsync_CallerWithEditAssociations_Succeeds()
    {
        var caller = BuildUser(1, BuildRole(1, "Editor", EditAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city);
        var service = BuildService(caller, associations: [association], cities: [city]);

        var dto = new UpdateAssociationDto { Id = 10, EnglishName = "Updated", ArabicName = "محدثة", CityId = city.Id };

        var result = await service.UpdateAsync(caller.UserId, dto);

        Assert.Equal("Updated", result.EnglishName);
    }

    [Fact]
    public async Task DeleteAsync_CallerWithDeleteAssociations_SoftDeletes()
    {
        var caller = BuildUser(1, BuildRole(1, "Deleter", DeleteAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city);
        var (service, repository) = BuildServiceWithRepository(caller, associations: [association]);

        await service.DeleteAsync(caller.UserId, 10);

        Assert.True(association.IsDeleted);
        Assert.NotNull(repository.RemovedAssociation);
    }

    [Fact]
    public async Task DeleteAsync_CallerWithOnlyEditAssociations_ThrowsUnauthorizedAccessException()
    {
        var caller = BuildUser(1, BuildRole(1, "Editor", EditAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city);
        var service = BuildService(caller, associations: [association]);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(caller.UserId, 10));
    }

    [Fact]
    public async Task RestoreAsync_DeletedAssociation_ClearsIsDeleted()
    {
        var caller = BuildUser(1, BuildRole(1, "Deleter", DeleteAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city, isDeleted: true);
        var service = BuildService(caller, associations: [association]);

        var result = await service.RestoreAsync(caller.UserId, 10);

        Assert.False(result.IsDeleted);
        Assert.False(association.IsDeleted);
    }

    [Fact]
    public async Task RestoreAsync_NotDeleted_ThrowsInvalidOperationException()
    {
        var caller = BuildUser(1, BuildRole(1, "Deleter", DeleteAssociations));
        var country = BuildCountry(1);
        var city = BuildCity(1, country);
        var association = BuildAssociation(10, city, isDeleted: false);
        var service = BuildService(caller, associations: [association]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RestoreAsync(caller.UserId, 10));
    }

    private static AssociationService BuildService(User caller, Association[]? associations = null, City[]? cities = null) =>
        BuildServiceWithRepository(caller, associations, cities).Service;

    private static (AssociationService Service, FakeAssociationRepository Repository) BuildServiceWithRepository(User caller, Association[]? associations = null, City[]? cities = null)
    {
        var userRepository = new FakeUserRepository(caller);
        var associationRepository = new FakeAssociationRepository(associations ?? [], cities ?? []);
        var cityRepository = new FakeCityRepository(cities ?? []);

        var unitOfWork = new FakeUnitOfWork(userRepository, associationRepository, cityRepository);
        return (new AssociationService(unitOfWork), associationRepository);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User _currentUser;
        public FakeUserRepository(User currentUser) => _currentUser = currentUser;

        public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(userId == _currentUser.UserId ? _currentUser : null);

        public Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(userId == _currentUser.UserId ? _currentUser : null);

        public Task<User?> GetByEmailWithAccessAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByEntityAsync(EntityType entityType, int entityId, string? search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsReadOnlyAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByIdsInEntityAsync(IEnumerable<int> userIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<User> Users, int TotalCount)> GetPagedByEntityAsync(EntityType entityType, int entityId, string? search, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(User user) => throw new NotSupportedException();
    }

    private sealed class FakeCityRepository : ICityRepository
    {
        private readonly Dictionary<int, City> _citiesById;
        public FakeCityRepository(IEnumerable<City> cities) => _citiesById = cities.ToDictionary(c => c.Id);

        public Task<City?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_citiesById.TryGetValue(cityId, out var city) && !city.IsDeleted ? city : null);

        public Task<City?> GetByIdIncludingDeletedAsync(int cityId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<City>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(City city, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(City city) => throw new NotSupportedException();
    }

    // Remove() flips IsDeleted directly, same simulated soft-delete as every other fake repository
    // in this test project (there's no real change-tracking pipeline here to intercept it for us).
    private sealed class FakeAssociationRepository : IAssociationRepository
    {
        private readonly Dictionary<int, Association> _associationsById;
        private readonly Dictionary<int, City> _citiesById;

        public FakeAssociationRepository(IEnumerable<Association> associations, IEnumerable<City>? cities = null)
        {
            _associationsById = associations.ToDictionary(a => a.Id);
            _citiesById = (cities ?? Enumerable.Empty<City>()).ToDictionary(c => c.Id);
        }

        public Association? RemovedAssociation { get; private set; }

        public Task<Association?> GetByIdAsync(int associationId, CancellationToken cancellationToken = default)
        {
            var association = _associationsById.GetValueOrDefault(associationId);
            return Task.FromResult(association is { IsDeleted: false } ? association : null);
        }

        public Task<Association?> GetByIdIncludingDeletedAsync(int associationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_associationsById.GetValueOrDefault(associationId));

        public Task<(Association Association, int WorkersCount)?> GetByIdWithWorkersCountAsync(int associationId, CancellationToken cancellationToken = default)
        {
            var association = _associationsById.GetValueOrDefault(associationId);
            return Task.FromResult(association is { IsDeleted: false } ? (association, 0) : ((Association, int)?)null);
        }

        public Task<List<(Association Association, int WorkersCount)>> QueryAsync(bool isDeleted, string? searchTerm = null, bool? orderByWorkersCountAscending = null, CancellationToken cancellationToken = default)
        {
            var results = _associationsById.Values.Where(a => a.IsDeleted == isDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                results = results.Where(a => a.EnglishName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || a.ArabicName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(results.Select(a => (a, 0)).ToList());
        }

        public Task<List<Association>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task AddAsync(Association association, CancellationToken cancellationToken = default)
        {
            association.Id = association.Id == 0 ? _associationsById.Count + 100 : association.Id;
            // Real EF Core performs City navigation fixup automatically once both entities are
            // tracked in the same DbContext (AssociationService.CreateAsync only sets CityId, not
            // .City) — this plain in-memory fake has no change tracker, so it's done by hand here,
            // matching real runtime behavior, for MapToDto (called right after AddAsync via
            // MapExistingAsync) to have a non-null City/Country to read.
            if (_citiesById.TryGetValue(association.CityId, out var city))
            {
                association.City = city;
            }

            _associationsById[association.Id] = association;
            return Task.CompletedTask;
        }

        public void Remove(Association association)
        {
            association.IsDeleted = true;
            RemovedAssociation = association;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IUserRepository users, IAssociationRepository associations, ICityRepository cities)
        {
            Users = users;
            Associations = associations;
            Cities = cities;
        }

        public IUserRepository Users { get; }
        public IAssociationRepository Associations { get; }
        public ICityRepository Cities { get; }
        public IRoleRepository Roles => throw new NotSupportedException();
        public IPermissionRepository Permissions => throw new NotSupportedException();
        public IGroupRepository Groups => throw new NotSupportedException();
        public IProjectTypeRepository ProjectTypes => throw new NotSupportedException();
        public ICountryRepository Countries => throw new NotSupportedException();
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

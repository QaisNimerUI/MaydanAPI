using Maydan.Application.DTOs.UserManagement;
using Maydan.Application.Interfaces;
using Maydan.Application.Services;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Tests.Services;

// Audit follow-up (entityType/entityId mis-scoping found during the RBAC ticket's live
// verification): CreateUserAsync always stamps the new user's EntityType/EntityId from the
// CALLER, never from dto.RoleId. That's correct for same-role creation (the only case the real UI
// ever sends) but was silently wrong for a cross-role call. These tests prove
// UserManagementService.EnsureSameEntityCreation() now rejects the cross-role case instead of
// silently mis-scoping it, while leaving the same-role, self-service path untouched.
public class UserManagementServiceTests
{
    [Fact]
    public async Task CreateUserAsync_SameRoleAsCreator_SucceedsAndInheritsCreatorsEntity()
    {
        var associationRole = new Role { RoleId = 4, RoleNameEn = "Association", RoleNameAr = "الجمعية" };
        var currentUser = new User
        {
            UserId = 1,
            RoleId = associationRole.RoleId,
            Role = associationRole,
            EntityType = EntityType.Association,
            EntityId = 7,
            IsActive = true
        };

        var userRepository = new FakeUserRepository(currentUser, associationRole);
        var unitOfWork = new FakeUnitOfWork(userRepository, new FakeRoleRepository(associationRole));
        var service = new UserManagementService(unitOfWork, new FakePasswordHasher());

        var dto = new CreateEntityUserDto
        {
            FirstNameEn = "New",
            LastNameEn = "Employee",
            FirstNameAr = "جديد",
            LastNameAr = "موظف",
            Email = "new.employee@example.org",
            PhoneNumber = "+962700000000",
            InitialPassword = "P@ssw0rd!",
            RoleId = associationRole.RoleId // same role as the creator
        };

        var result = await service.CreateUserAsync(currentUser.UserId, dto);

        Assert.Equal(EntityType.Association, result.EntityType);
        Assert.Equal(7, result.EntityId);
        Assert.NotNull(userRepository.AddedUser);
        Assert.Equal(EntityType.Association, userRepository.AddedUser!.EntityType);
        Assert.Equal(7, userRepository.AddedUser.EntityId);
    }

    [Fact]
    public async Task CreateUserAsync_DifferentRoleThanCreator_ThrowsAndNeverCreatesTheUser()
    {
        var baytAlUrdonRole = new Role { RoleId = 1, RoleNameEn = "Bayt-AlUrdon", RoleNameAr = "بيت الأردن" };
        var currentUser = new User
        {
            UserId = 1,
            RoleId = baytAlUrdonRole.RoleId,
            Role = baytAlUrdonRole,
            EntityType = EntityType.BaytAlUrdon,
            EntityId = 1,
            IsActive = true
        };

        var userRepository = new FakeUserRepository(currentUser, baytAlUrdonRole);
        // The role catalog lookup is never reached — EnsureSameEntityCreation rejects before it —
        // so the repository only needs to know the creator's own role.
        var unitOfWork = new FakeUnitOfWork(userRepository, new FakeRoleRepository(baytAlUrdonRole));
        var service = new UserManagementService(unitOfWork, new FakePasswordHasher());

        var dto = new CreateEntityUserDto
        {
            FirstNameEn = "Cross",
            LastNameEn = "Role",
            FirstNameAr = "عبر",
            LastNameAr = "دور",
            Email = "cross.role@example.org",
            PhoneNumber = "+962700000001",
            InitialPassword = "P@ssw0rd!",
            RoleId = 4 // Association — different from the caller's own role (1, Bayt-AlUrdon)
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUserAsync(currentUser.UserId, dto));

        Assert.Contains("different role", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(userRepository.AddedUser);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<int, User> _usersById;
        private readonly Role _roleForNewUsers;

        public FakeUserRepository(User currentUser, Role roleForNewUsers)
        {
            _usersById = new Dictionary<int, User> { [currentUser.UserId] = currentUser };
            _roleForNewUsers = roleForNewUsers;
        }

        public User? AddedUser { get; private set; }

        public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usersById.TryGetValue(userId, out var user) ? user : null);

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            user.UserId = 100;
            user.Role = _roleForNewUsers;
            AddedUser = user;
            _usersById[user.UserId] = user;
            return Task.CompletedTask;
        }

        public Task<User?> GetDetailsReadOnlyAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usersById.TryGetValue(userId, out var user) ? user : null);

        // Not exercised by either test above.
        public Task<User?> GetByEmailWithAccessAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByEntityAsync(EntityType entityType, int entityId, string? search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByIdsInEntityAsync(IEnumerable<int> userIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(User user) => throw new NotSupportedException();
    }

    private sealed class FakeRoleRepository : IRoleRepository
    {
        private readonly Dictionary<int, Role> _rolesById;

        public FakeRoleRepository(params Role[] roles) => _rolesById = roles.ToDictionary(r => r.RoleId);

        public Task<Role?> GetWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_rolesById.TryGetValue(roleId, out var role) ? role : null);

        // Not exercised by either test above.
        public Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(Role role, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(Role role) => throw new NotSupportedException();
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed:{password}";
        public bool VerifyPassword(string password, string passwordHash) => passwordHash == $"hashed:{password}";
    }

    // Only Users/Roles are backed by a working fake — CreateUserAsync never reaches the other
    // repositories in either test above (both DTOs use empty PermissionIds/GroupIds, so
    // EnsurePermissionsExistAsync/EnsureGroupsInEntityAsync return before touching them).
    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IUserRepository users, IRoleRepository roles)
        {
            Users = users;
            Roles = roles;
        }

        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IPermissionRepository Permissions => throw new NotSupportedException();
        public IGroupRepository Groups => throw new NotSupportedException();
        public IProjectTypeRepository ProjectTypes => throw new NotSupportedException();
        public ICountryRepository Countries => throw new NotSupportedException();
        public ICityRepository Cities => throw new NotSupportedException();
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

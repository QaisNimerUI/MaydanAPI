using Maydan.Application.DTOs.Auth;
using Maydan.Application.Interfaces;
using Maydan.Application.Services;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;
using Maydan.Infrastructure.Security;

namespace Maydan.Application.Tests.Services;

// Forgot-password recovery (MAYD-128/129/130, 2026-09-23): uses the REAL PasswordHasher (not a
// fake) — the whole point of these tests is proving the actual salted-hash generate/verify round
// trip works for reset tokens the same way it already does for passwords (see
// PasswordResetToken.cs's own comment on why a token lookup can't be a direct hash-equality query).
public class AuthServiceTests
{
    private static Role AssociationRole() => new() { RoleId = 4, RoleNameEn = "Association", RoleNameAr = "الجمعية" };

    private static User ActiveUser(int userId = 1, string email = "user@example.org", bool mustResetPassword = false) => new()
    {
        UserId = userId,
        Email = email,
        FirstNameEn = "Test",
        LastNameEn = "User",
        FirstNameAr = "اختبار",
        LastNameAr = "مستخدم",
        PhoneNumber = "+962700000000",
        PasswordHash = new PasswordHasher().HashPassword("OldP@ssw0rd!"),
        MustResetPassword = mustResetPassword,
        IsActive = true,
        RoleId = 4,
        Role = AssociationRole(),
        EntityType = EntityType.Association,
        EntityId = 1
    };

    private static (AuthService service, FakeUserRepository users, FakePasswordResetTokenRepository tokens, FakeEmailSender emailSender, FakeFrontendLinkBuilder linkBuilder)
        CreateService(User? existingUser = null)
    {
        var users = new FakeUserRepository(existingUser);
        var tokens = new FakePasswordResetTokenRepository();
        var emailSender = new FakeEmailSender();
        var linkBuilder = new FakeFrontendLinkBuilder();
        var unitOfWork = new FakeUnitOfWork(users, tokens);
        var service = new AuthService(unitOfWork, new PasswordHasher(), new FakeJwtTokenGenerator(), emailSender, linkBuilder);

        return (service, users, tokens, emailSender, linkBuilder);
    }

    private static string ExtractToken(string link) => Uri.UnescapeDataString(link.Split("token=")[1]);

    public class ForgotPasswordAsyncTests
    {
        [Fact]
        public async Task ExistingActiveUser_CreatesAHashedTokenAndEmailsARealResetLink()
        {
            var user = ActiveUser();
            var (service, users, tokens, emailSender, _) = CreateService(user);

            var result = await service.ForgotPasswordAsync(new ForgotPasswordDto(user.Email));

            Assert.NotNull(tokens.AddedToken);
            Assert.Equal(user.UserId, tokens.AddedToken!.UserId);
            Assert.False(tokens.AddedToken.IsUsed);
            // Not the raw token — a salted hash of it (PasswordHasher's own format), same as passwords.
            Assert.DoesNotContain(ExtractToken(emailSender.LastLink!), tokens.AddedToken.TokenHash);

            Assert.Equal(1, emailSender.SendCallCount);
            Assert.Equal(user.Email, emailSender.LastToEmail);
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task TokenGenerated_HashVerifiesAgainstTheRawTokenEmailedToTheUser()
        {
            var user = ActiveUser();
            var (service, _, tokens, emailSender, _) = CreateService(user);

            await service.ForgotPasswordAsync(new ForgotPasswordDto(user.Email));

            var rawToken = ExtractToken(emailSender.LastLink!);
            var hasher = new PasswordHasher();
            Assert.True(hasher.VerifyPassword(rawToken, tokens.AddedToken!.TokenHash));
        }

        [Fact]
        public async Task SetsExpiryTo24HoursFromNow()
        {
            var user = ActiveUser();
            var (service, _, tokens, _, _) = CreateService(user);
            var before = DateTime.UtcNow;

            await service.ForgotPasswordAsync(new ForgotPasswordDto(user.Email));

            var after = DateTime.UtcNow;
            Assert.InRange(tokens.AddedToken!.ExpiresAtUtc, before.AddHours(24).AddSeconds(-5), after.AddHours(24).AddSeconds(5));
        }

        [Fact]
        public async Task UnknownEmail_ReturnsTheSameGenericMessageAndSendsNoEmail_EnumerationSafety()
        {
            var (existingService, _, _, existingEmailSender, _) = CreateService(ActiveUser(email: "real@example.org"));
            var existingResult = await existingService.ForgotPasswordAsync(new ForgotPasswordDto("real@example.org"));

            var (unknownService, _, unknownTokens, unknownEmailSender, _) = CreateService(existingUser: null);
            var unknownResult = await unknownService.ForgotPasswordAsync(new ForgotPasswordDto("nobody@example.org"));

            Assert.Equal(existingResult.Message, unknownResult.Message);
            Assert.Equal(0, unknownEmailSender.SendCallCount);
            Assert.Null(unknownTokens.AddedToken);
        }

        [Fact]
        public async Task InactiveUser_TreatedTheSameAsUnknownEmail_EnumerationSafety()
        {
            var inactiveUser = ActiveUser();
            inactiveUser.IsActive = false;
            var (service, _, tokens, emailSender, _) = CreateService(inactiveUser);

            var result = await service.ForgotPasswordAsync(new ForgotPasswordDto(inactiveUser.Email));

            Assert.Equal(0, emailSender.SendCallCount);
            Assert.Null(tokens.AddedToken);
            Assert.Contains("if an account", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task BlankEmail_ThrowsARealValidationError_NotTheEnumerationSafeMessage()
        {
            var (service, _, _, _, _) = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ForgotPasswordAsync(new ForgotPasswordDto("")));
        }

        // Decision confirmed 2026-09-23: requesting a new link invalidates any earlier unused,
        // unexpired token for that same user (reusing IsUsed rather than adding a separate
        // "superseded" reason — ResetPasswordWithTokenAsync's existing IsUsed check already rejects
        // it with the same real "already used" message, no new error path needed). Otherwise an old,
        // forgotten link sitting in an inbox or browser history would stay usable right alongside a
        // newer one until it separately expired.
        [Fact]
        public async Task SecondRequest_InvalidatesTheFirstStillOutstandingToken_OldOneRejectedNewOneStillWorks()
        {
            var user = ActiveUser();
            var (service, _, tokens, emailSender, _) = CreateService(user);

            await service.ForgotPasswordAsync(new ForgotPasswordDto(user.Email));
            var firstRawToken = ExtractToken(emailSender.SentLinks[0]);

            await service.ForgotPasswordAsync(new ForgotPasswordDto(user.Email));
            var secondRawToken = ExtractToken(emailSender.SentLinks[1]);

            Assert.Equal(2, tokens.AllTokens.Count);
            Assert.True(tokens.AllTokens[0].IsUsed); // superseded by the second request
            Assert.False(tokens.AllTokens[1].IsUsed); // the one actually just issued

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(firstRawToken, "NewP@ssw0rd!", "NewP@ssw0rd!")));
            Assert.Contains("already", exception.Message, StringComparison.OrdinalIgnoreCase);

            var result = await service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(secondRawToken, "NewP@ssw0rd!", "NewP@ssw0rd!"));
            Assert.Contains("reset", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ResetPasswordWithTokenAsyncTests
    {
        private static async Task<(AuthService service, FakeUserRepository users, FakePasswordResetTokenRepository tokens, string rawToken, User user)>
            CreateServiceWithAnIssuedToken(bool mustResetPassword = false)
        {
            var user = ActiveUser(mustResetPassword: mustResetPassword);
            var (service, users, tokens, emailSender, _) = CreateService(user);
            await service.ForgotPasswordAsync(new ForgotPasswordDto(user.Email));
            var rawToken = ExtractToken(emailSender.LastLink!);

            return (service, users, tokens, rawToken, user);
        }

        [Fact]
        public async Task ValidToken_ResetsThePasswordAndMarksTheTokenUsed()
        {
            var (service, users, tokens, rawToken, user) = await CreateServiceWithAnIssuedToken();

            var result = await service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "NewP@ssw0rd!", "NewP@ssw0rd!"));

            Assert.True(tokens.AddedToken!.IsUsed);
            Assert.NotNull(tokens.AddedToken.UsedAtUtc);
            Assert.True(new PasswordHasher().VerifyPassword("NewP@ssw0rd!", users.TrackedUser!.PasswordHash));
            Assert.False(users.TrackedUser.MustResetPassword);
            Assert.Contains("reset", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ValidToken_AlsoClearsMustResetPassword_SoTheUserIsNotForcedThroughThatFlowRightAfter()
        {
            var (service, users, _, rawToken, _) = await CreateServiceWithAnIssuedToken(mustResetPassword: true);

            await service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "NewP@ssw0rd!", "NewP@ssw0rd!"));

            Assert.False(users.TrackedUser!.MustResetPassword);
        }

        [Fact]
        public async Task OldPasswordNoLongerVerifiesAfterAReset_TheStorysOwnFirstAcceptanceCriterion()
        {
            var (service, users, _, rawToken, _) = await CreateServiceWithAnIssuedToken();

            await service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "NewP@ssw0rd!", "NewP@ssw0rd!"));

            Assert.False(new PasswordHasher().VerifyPassword("OldP@ssw0rd!", users.TrackedUser!.PasswordHash));
        }

        [Fact]
        public async Task UsedToken_RejectedOnASecondAttempt_WithADistinctAlreadyUsedMessage()
        {
            var (service, _, _, rawToken, _) = await CreateServiceWithAnIssuedToken();
            await service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "NewP@ssw0rd!", "NewP@ssw0rd!"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "AnotherP@ss1!", "AnotherP@ss1!")));

            Assert.Contains("already", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExpiredToken_RejectedWithADistinctExpiredMessage()
        {
            var (service, _, tokens, rawToken, _) = await CreateServiceWithAnIssuedToken();
            tokens.AddedToken!.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "NewP@ssw0rd!", "NewP@ssw0rd!")));

            Assert.Contains("expired", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UnknownToken_RejectedWithADistinctInvalidMessage()
        {
            var (service, _, _, _, _) = await CreateServiceWithAnIssuedToken();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto("not-a-real-token", "NewP@ssw0rd!", "NewP@ssw0rd!")));

            Assert.Contains("invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task MismatchedConfirmation_Rejected()
        {
            var (service, _, _, rawToken, _) = await CreateServiceWithAnIssuedToken();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResetPasswordWithTokenAsync(new ResetPasswordWithTokenDto(rawToken, "NewP@ssw0rd!", "Different1!")));

            Assert.Contains("match", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

        public FakeUserRepository(User? user) => _user = user;

        public User? TrackedUser { get; private set; }

        public Task<User?> GetByEmailWithAccessAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_user is not null && string.Equals(_user.Email, email, StringComparison.OrdinalIgnoreCase) ? _user : null);

        public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (_user is null || _user.UserId != userId)
            {
                return Task.FromResult<User?>(null);
            }

            TrackedUser = _user;
            return Task.FromResult<User?>(_user);
        }

        public Task<List<User>> GetByEntityAsync(EntityType entityType, int entityId, string? search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetDetailsReadOnlyAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<User>> GetByIdsInEntityAsync(IEnumerable<int> userIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(User user, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(User user) => throw new NotSupportedException();
    }

    // Backs GetAllAsync with every token ever added (not just the latest) — needed to exercise
    // ForgotPasswordAsync's own invalidate-outstanding-tokens step, which reads the full list back
    // via the same GetAllAsync the real repository uses. AddedToken stays as an alias for the most
    // recently added token, since every pre-existing test only ever adds one.
    private sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly List<PasswordResetToken> _tokens = new();

        public IReadOnlyList<PasswordResetToken> AllTokens => _tokens;
        public PasswordResetToken? AddedToken => _tokens.Count == 0 ? null : _tokens[^1];

        public Task<List<PasswordResetToken>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new List<PasswordResetToken>(_tokens));

        public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            token.Id = _tokens.Count + 1;
            _tokens.Add(token);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        private readonly List<string> _sentLinks = new();

        public int SendCallCount { get; private set; }
        public string? LastToEmail { get; private set; }
        public string? LastLink => _sentLinks.Count == 0 ? null : _sentLinks[^1];
        public IReadOnlyList<string> SentLinks => _sentLinks;

        public Task SendAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
        {
            SendCallCount++;
            LastToEmail = toEmail;
            var start = bodyHtml.IndexOf("http", StringComparison.Ordinal);
            var end = bodyHtml.IndexOf('"', start);
            _sentLinks.Add(bodyHtml[start..end]);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeFrontendLinkBuilder : IFrontendLinkBuilder
    {
        public string BuildResetPasswordLink(string token) => $"http://localhost:4200/auth/reset-password-with-token?token={Uri.EscapeDataString(token)}";
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(AuthUserDto user) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IUserRepository users, IPasswordResetTokenRepository passwordResetTokens)
        {
            Users = users;
            PasswordResetTokens = passwordResetTokens;
        }

        public IUserRepository Users { get; }
        public IPasswordResetTokenRepository PasswordResetTokens { get; }
        public IRoleRepository Roles => throw new NotSupportedException();
        public IPermissionRepository Permissions => throw new NotSupportedException();
        public IGroupRepository Groups => throw new NotSupportedException();
        public IProjectTypeRepository ProjectTypes => throw new NotSupportedException();
        public ICountryRepository Countries => throw new NotSupportedException();
        public ICityRepository Cities => throw new NotSupportedException();
        public IAssociationRepository Associations => throw new NotSupportedException();
        public IProductionCompanyRepository ProductionCompanies => throw new NotSupportedException();
        public IProjectRepository Projects => throw new NotSupportedException();
        public IWorkerRepository Workers => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default) => operation();
    }
}

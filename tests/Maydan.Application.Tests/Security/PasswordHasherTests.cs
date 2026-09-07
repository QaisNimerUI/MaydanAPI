using Maydan.Infrastructure.Security;

namespace Maydan.Application.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ReturnsHashThatDoesNotContainPlainTextPassword()
    {
        var hasher = new PasswordHasher();

        var hash = hasher.HashPassword("P@ssw0rd!");

        Assert.DoesNotContain("P@ssw0rd!", hash);
        Assert.StartsWith("PBKDF2-SHA256.", hash);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_WhenPasswordMatchesHash()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.HashPassword("P@ssw0rd!");

        var result = hasher.VerifyPassword("P@ssw0rd!", hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_WhenPasswordDoesNotMatchHash()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.HashPassword("P@ssw0rd!");

        var result = hasher.VerifyPassword("WrongPassword", hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_WhenHashFormatIsInvalid()
    {
        var hasher = new PasswordHasher();

        var result = hasher.VerifyPassword("P@ssw0rd!", "invalid-hash");

        Assert.False(result);
    }
}

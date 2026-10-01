using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Maydan.Infrastructure.Security;

// System Configuration gate (MAYD-133, 2026-09-24) — see ISecretProtector's own comment for why
// this exists and why it's Data Protection specifically (no existing encryption-at-rest pattern
// was found anywhere in this codebase to reuse instead).
//
// Purpose string is a fixed, versioned key so ciphertext stays decryptable across app restarts as
// long as the Data Protection key ring persists — by default (no builder.Services.AddDataProtection()
// persistence configuration in Program.cs) ASP.NET Core stores keys under the local user profile,
// which is fine for a single-instance Dev/QA box but will NOT survive across multiple instances or
// containers without shared key storage configured (e.g. PersistKeysToFileShare/PersistKeysToDbContext/
// Redis) — flagged in this ticket's report; out of scope to configure without knowing the real
// deployment topology.
public class DataProtectionSecretProtector : ISecretProtector
{
    private const string Purpose = "Maydan.SystemConfiguration.Smtp.v1";

    private readonly IDataProtector _protector;

    public DataProtectionSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}

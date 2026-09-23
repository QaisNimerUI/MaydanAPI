namespace Maydan.Application.Interfaces;

// System Configuration gate (MAYD-133, 2026-09-24): reversible encryption for secrets that must be
// read back in plaintext later (the SMTP password, to actually authenticate with the SMTP server) —
// unlike IPasswordHasher, which is one-way and can only verify, never recover, a value.
//
// No existing encryption-at-rest pattern was found anywhere in this codebase before this ticket —
// grepped for IDataProtector/DataProtection/Aes/RSA/ProtectedData across the whole repo and found
// nothing real. Worker.CivilId's own comment claims "encrypted at rest with a non-deterministic
// scheme", but no such implementation actually exists (only HmacCivilIdHasher, a one-way blind-index
// hash, same category as IPasswordHasher — not reusable here either). Flagged to Yousef in this
// ticket's report rather than silently defaulting to plaintext or inventing ad-hoc crypto.
//
// DataProtectionSecretProtector (Infrastructure) is the chosen interim implementation: ASP.NET
// Core's own built-in Data Protection API, not a custom scheme — the framework's standard,
// already-audited mechanism for exactly this ("store a secret, read it back later"), zero
// hand-rolled crypto code. Confirm/revisit with Yousef, especially key-persistence configuration
// (see that class's own comment) if this API ever runs across multiple instances/containers.
public interface ISecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}

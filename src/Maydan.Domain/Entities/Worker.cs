using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class Worker : SharedEntities
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;

    // Repo-hygiene fix (2026-09-26): this comment used to claim CivilId is "encrypted at rest with
    // a non-deterministic scheme" — that was never actually implemented anywhere in this codebase
    // (confirmed by grepping the whole repo for Protect/Unprotect/IDataProtector touching CivilId —
    // nothing real). CivilId is currently stored as PLAINTEXT. Only CivilIdHash (below) exists
    // today, and it's a one-way blind index, not encryption — it can verify/look up a match but can
    // never recover the original CivilId. First flagged in ISecretProtector.cs's own comment
    // (MAYD-133/Stage4) and never acted on until now; there is also no WorkerService/
    // WorkersController yet, so nothing currently sets a real CivilId value at all — this is a
    // false comment sitting on a skeleton, not a live plaintext leak in a running feature.
    //
    // The real fix for whoever builds the actual Workers create/update flow: encrypt CivilId via
    // the existing ISecretProtector.Protect() before persisting, and ISecretProtector.Unprotect()
    // when reading it back for display — reuse the exact mechanism already built and in production
    // use for SystemConfiguration.SmtpPasswordProtected, don't invent a second encryption scheme.
    // Two things to get right when that's wired up:
    //   1. DataProtectionSecretProtector's current DI registration bakes in a single fixed purpose
    //      string ("Maydan.SystemConfiguration.Smtp.v1"). CivilId must NOT be protected under that
    //      same purpose — Data Protection purposes exist specifically to key-separate different data
    //      categories. Either register a second ISecretProtector-shaped instance with its own
    //      purpose (e.g. "Maydan.Worker.CivilId.v1") or extend the interface to accept a purpose.
    //   2. The same key-persistence caveat DataProtectionSecretProtector's own comment documents
    //      applies here too: by default (no AddDataProtection() persistence configuration in
    //      Program.cs) keys are stored under the local user profile — fine for a single-instance
    //      Dev/QA box, but ciphertext becomes undecryptable across instances/containers without
    //      shared key storage (file share/DB/Redis) configured first.
    //
    // No unique constraint on this column (see WorkerConfiguration's own comment) — uniqueness/
    // lookup goes through CivilIdHash (blind index) instead, since ciphertext won't be directly
    // queryable/comparable even once encryption is actually implemented.
    public string CivilId { get; set; } = string.Empty;

    // HMAC-SHA256(CivilId) with a server-side secret key. One-way and deterministic — safe to index
    // and query on for uniqueness/lookup (see IWorkerRepository.GetByCivilIdHashAsync), and unlike
    // CivilId itself, this value never needs to be decrypted back to the original.
    public string CivilIdHash { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }

    public int AssociationId { get; set; }
    public Association Association { get; set; } = null!;

    public string QrCode { get; set; } = string.Empty;
    // Deliberately excluded: DailyWageAmount — belongs on the future ProjectWorker
    // assignment, not the worker themselves (deferred with the Service Request system).
}

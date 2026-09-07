using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class Worker : SharedEntities
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;

    // Encrypted at rest with a non-deterministic scheme — no unique constraint here.
    // Uniqueness/lookup goes through CivilIdHash (blind index) instead.
    public string CivilId { get; set; } = string.Empty;

    // HMAC-SHA256(CivilId) with a server-side secret key. Deterministic, so it's safe to
    // index and query on even though CivilId itself is randomly encrypted.
    public string CivilIdHash { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }

    public int AssociationId { get; set; }
    public Association Association { get; set; } = null!;

    public string QrCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Deliberately excluded: DailyWageAmount — belongs on the future ProjectWorker
    // assignment, not the worker themselves (deferred with the Service Request system).
}

using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IAssociationRepository
{
    // Unchanged signatures — EntityOnboardingService (the real, already-built associations/
    // without-admin + associations/{id}/admin flow) already depends on these exact shapes; Phase 2a
    // only adds to this interface, never changes what's already here.
    Task<Association?> GetByIdAsync(int associationId, CancellationToken cancellationToken = default);
    Task<List<Association>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Association association, CancellationToken cancellationToken = default);
    void Remove(Association association);

    // Bypasses the soft-delete filter — needed by RestoreAsync, same pattern as
    // ICountryRepository.GetByIdIncludingDeletedAsync (Phase 1's own precedent).
    Task<Association?> GetByIdIncludingDeletedAsync(int associationId, CancellationToken cancellationToken = default);

    // GET /api/Associations/{id} needs WorkersCount alongside the entity itself — a computed
    // COUNT(Workers WHERE AssociationId = Id) (Association.cs's own comment), not a stored column,
    // so it's carried out of the repository as a tuple rather than bolted onto the entity.
    Task<(Association Association, int WorkersCount)?> GetByIdWithWorkersCountAsync(int associationId, CancellationToken cancellationToken = default);

    // One flexible query method covering every list-shaped endpoint this module needs (GetAll,
    // by-name search, order-by-worker-count asc/desc, the deleted-only list, and deleted+search
    // combined) — same "one method, optional params" shape ProjectRepository.GetAllAsync already
    // established, rather than five near-duplicate methods. isDeleted=true bypasses the soft-delete
    // filter and returns ONLY deleted rows (mirrors ProjectRepository's own isDeleted-toggle
    // comment); searchTerm matches EnglishName OR ArabicName (same Contains-on-both-fields
    // convention as UserRepository's own name search);
    // orderByWorkersCountAscending null falls back to EnglishName ordering.
    Task<List<(Association Association, int WorkersCount)>> QueryAsync(
        bool isDeleted,
        string? searchTerm = null,
        bool? orderByWorkersCountAscending = null,
        CancellationToken cancellationToken = default);
}

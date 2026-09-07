using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IWorkerRepository
{
    Task<Worker?> GetByIdAsync(int workerId, CancellationToken cancellationToken = default);

    // civilIdHash is the blind index (ICivilIdHasher.ComputeHash) — CivilId itself is encrypted
    // non-deterministically and can't be queried directly.
    Task<Worker?> GetByCivilIdHashAsync(string civilIdHash, CancellationToken cancellationToken = default);
    Task<List<Worker>> GetByAssociationIdAsync(int associationId, CancellationToken cancellationToken = default);
    Task AddAsync(Worker worker, CancellationToken cancellationToken = default);
    void Remove(Worker worker);
}

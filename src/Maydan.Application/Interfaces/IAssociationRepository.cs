using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IAssociationRepository
{
    Task<Association?> GetByIdAsync(int associationId, CancellationToken cancellationToken = default);
    Task<List<Association>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Association association, CancellationToken cancellationToken = default);
    void Remove(Association association);
}

using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<List<Group>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    void Remove(Group group);
}

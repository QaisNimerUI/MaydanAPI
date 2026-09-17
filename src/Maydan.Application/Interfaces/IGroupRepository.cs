using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Group?> GetDetailsAsync(int groupId, CancellationToken cancellationToken = default);
    Task<List<Group>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Group>> GetByEntityAsync(EntityType entityType, int entityId, CancellationToken cancellationToken = default);
    Task<List<Group>> GetByIdsInEntityAsync(IEnumerable<int> groupIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsInEntityAsync(EntityType entityType, int entityId, string groupNameEn, string groupNameAr, int? excludedGroupId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    void Remove(Group group);
}

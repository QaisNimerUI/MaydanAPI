using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maydan.Application.Interfaces
{
    public interface IProjectTypeRepository
    {
        Task<bool> ExistsAsync(
            int projectTypeId,
            CancellationToken cancellationToken = default);

        Task<List<Maydan.Domain.Entities.ProjectType>> GetAllAsync(
            CancellationToken cancellationToken = default);
    }
}

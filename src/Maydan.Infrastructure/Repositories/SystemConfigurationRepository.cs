using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class SystemConfigurationRepository : ISystemConfigurationRepository
{
    private readonly MaydanDbContext _context;

    public SystemConfigurationRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<SystemConfiguration?> GetAsync(CancellationToken cancellationToken = default) =>
        _context.SystemConfigurations.FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default) =>
        await _context.SystemConfigurations.AddAsync(configuration, cancellationToken);
}

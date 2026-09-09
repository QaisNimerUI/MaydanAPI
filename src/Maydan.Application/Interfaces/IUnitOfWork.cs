namespace Maydan.Application.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    IGroupRepository Groups { get; }
    IProjectTypeRepository ProjectTypes { get; }
    ICountryRepository Countries { get; }
    ICityRepository Cities { get; }
    IAssociationRepository Associations { get; }
    IProductionCompanyRepository ProductionCompanies { get; }
    IProjectRepository Projects { get; }
    IWorkerRepository Workers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

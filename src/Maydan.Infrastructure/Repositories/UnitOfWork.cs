using Maydan.Application.Interfaces;
using Maydan.Infrastructure.Persistence;

namespace Maydan.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MaydanDbContext _context;

    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IPermissionRepository? _permissions;
    private IGroupRepository? _groups;
    private ICountryRepository? _countries;
    private ICityRepository? _cities;
    private IAssociationRepository? _associations;
    private IProductionCompanyRepository? _productionCompanies;
    private IProjectRepository? _projects;
    private IWorkerRepository? _workers;

    public UnitOfWork(MaydanDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);
    public IGroupRepository Groups => _groups ??= new GroupRepository(_context);
    public ICountryRepository Countries => _countries ??= new CountryRepository(_context);
    public ICityRepository Cities => _cities ??= new CityRepository(_context);
    public IAssociationRepository Associations => _associations ??= new AssociationRepository(_context);
    public IProductionCompanyRepository ProductionCompanies => _productionCompanies ??= new ProductionCompanyRepository(_context);
    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
    public IWorkerRepository Workers => _workers ??= new WorkerRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

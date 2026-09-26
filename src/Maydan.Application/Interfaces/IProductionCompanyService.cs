using Maydan.Application.DTOs.ProductionCompanies;

namespace Maydan.Application.Interfaces;

// Production House Management (MAYD-80/81/82) — List, Details, and Activate/Deactivate for
// registered Production Companies. Does NOT include creation: MAYD-79's own public self-registration
// (ProductionCompanyOnboardingService) is the only real way a ProductionCompany gets created today;
// no MAYD ticket among 80/81/82 describes an internal admin create/edit form, so none is built here
// (company-form.component.ts stays the empty stub it already is — see this phase's own report).
public interface IProductionCompanyService
{
    Task<List<ProductionCompanyDto>> GetAllAsync(int currentUserId, string? search, bool isDeleted, CancellationToken cancellationToken = default);

    Task<ProductionCompanyDto> GetByIdAsync(int currentUserId, int productionCompanyId, CancellationToken cancellationToken = default);

    // MAYD-82: Manage-only (Super Admin/Bayt-AlUrdon), enforced here — see this method's own
    // implementation comment for the cascade-deactivate / non-cascade-reactivate design and why
    // it's deliberately asymmetric.
    Task<ProductionCompanyDto> UpdateStatusAsync(int currentUserId, int productionCompanyId, UpdateProductionCompanyStatusDto dto, CancellationToken cancellationToken = default);
}

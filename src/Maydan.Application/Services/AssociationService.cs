using System.Globalization;
using Maydan.Application.DTOs.Associations;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;

namespace Maydan.Application.Services;

// Association Management, Phase 2a (MAYD-4, MAYD-40..54) — Rima Bshara already built the real
// frontend (workforcment/src/app/features/associations/); only the backend was missing.
// Association.cs, AssociationConfiguration.cs, and a bare IAssociationRepository/AssociationRepository
// (GetByIdAsync/GetAllAsync/AddAsync/Remove only) already existed — this class, AssociationsController,
// and the repository's search/sort/soft-delete-aware query additions are what Phase 2a actually adds.
//
// Deliberately NOT built here (see this pass's own completion report for the full list): Association
// admin-creation (already exists — EntityOnboardingController's associations/without-admin +
// associations/{id}/admin, built during the onboarding phase; do not duplicate it), AssociationUsers
// linking (Phase 2b — no AssociationUser entity exists yet), cascading delete to Workers/AssociationUsers
// on Association delete (MAYD-51 — a separate business-rule decision, not resolved here, see
// DeleteAsync's own comment), CityLocations (a later phase — Association has no CityLocationId column;
// see AssociationDto's own comment), and the Service-Request-based edit/delete restriction (MAYD-48/50 —
// no ServiceRequest module exists anywhere in this codebase yet, confirmed by grep).
public class AssociationService : IAssociationService
{
    private const int MaxNameLength = 200;

    // Real permission catalog ids (PermissionSeedConfiguration.cs) — five distinct Association
    // permissions exist, not just View+Manage: ViewAssociations(9), CreateAssociations(10),
    // EditAssociations(11), DeleteAssociations(12), ManageAssociations(13). Confirmed via both the
    // backend seed (RolePermissionSeedConfiguration.cs) AND the real frontend's own permission gates
    // (associations-list.component.ts's canCreateAssociation/canViewAssociation/canEditAssociation/
    // canDeleteAssociation/canRestoreAssociation — each checks its own specific permission name OR
    // 'Manage Associations' as a universal override) that ManageAssociations is meant as a full
    // Create+Edit+Delete+View substitute, not a separate fifth capability — so every check below is
    // "the specific action's own permission OR ManageAssociations", matching the frontend exactly
    // rather than collapsing to a simpler two-permission model.
    private const int ViewAssociationsPermissionId = 9;
    private const int CreateAssociationsPermissionId = 10;
    private const int EditAssociationsPermissionId = 11;
    private const int DeleteAssociationsPermissionId = 12;
    private const int ManageAssociationsPermissionId = 13;

    private readonly IUnitOfWork _unitOfWork;

    public AssociationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AssociationDto>> GetAllAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, ViewAssociationsPermissionId, cancellationToken);

        var results = await _unitOfWork.Associations.QueryAsync(isDeleted: false, cancellationToken: cancellationToken);
        return results.Select(r => MapToDto(r.Association, r.WorkersCount)).ToList();
    }

    public async Task<AssociationDto> GetByIdAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, ViewAssociationsPermissionId, cancellationToken);

        var result = await _unitOfWork.Associations.GetByIdWithWorkersCountAsync(associationId, cancellationToken)
            ?? throw new KeyNotFoundException("Association was not found.");

        return MapToDto(result.Association, result.WorkersCount);
    }

    public async Task<List<AssociationDto>> SearchByNameAsync(int currentUserId, string name, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, ViewAssociationsPermissionId, cancellationToken);

        var results = await _unitOfWork.Associations.QueryAsync(isDeleted: false, searchTerm: name, cancellationToken: cancellationToken);
        return results.Select(r => MapToDto(r.Association, r.WorkersCount)).ToList();
    }

    public async Task<List<AssociationDto>> GetOrderedByWorkersCountAsync(int currentUserId, bool ascending, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, ViewAssociationsPermissionId, cancellationToken);

        var results = await _unitOfWork.Associations.QueryAsync(isDeleted: false, orderByWorkersCountAscending: ascending, cancellationToken: cancellationToken);
        return results.Select(r => MapToDto(r.Association, r.WorkersCount)).ToList();
    }

    public async Task<List<AssociationDto>> GetDeletedAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, ViewAssociationsPermissionId, cancellationToken);

        var results = await _unitOfWork.Associations.QueryAsync(isDeleted: true, cancellationToken: cancellationToken);
        return results.Select(r => MapToDto(r.Association, r.WorkersCount)).ToList();
    }

    public async Task<List<AssociationDto>> SearchDeletedByNameAsync(int currentUserId, string name, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, ViewAssociationsPermissionId, cancellationToken);

        var results = await _unitOfWork.Associations.QueryAsync(isDeleted: true, searchTerm: name, cancellationToken: cancellationToken);
        return results.Select(r => MapToDto(r.Association, r.WorkersCount)).ToList();
    }

    public async Task<AssociationDto> CreateAsync(int currentUserId, CreateAssociationDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetAuthorizedUserAsync(currentUserId, CreateAssociationsPermissionId, cancellationToken);

        ValidateName(dto.EnglishName, "English association name");
        ValidateName(dto.ArabicName, "Arabic association name");
        await EnsureCityExistsAsync(dto.CityId, cancellationToken);

        var association = new Association
        {
            EnglishName = dto.EnglishName.Trim(),
            ArabicName = dto.ArabicName.Trim(),
            CityId = dto.CityId,
            LocationOnGoogleMaps = string.IsNullOrWhiteSpace(dto.LocationOnGoogleMaps) ? null : dto.LocationOnGoogleMaps.Trim(),
            Latitude = ParseCoordinate(dto.Latitude, "Latitude"),
            Longitude = ParseCoordinate(dto.Longitude, "Longitude"),
            CreatedBy = currentUser.UserId,
            IsActive = true
        };

        await _unitOfWork.Associations.AddAsync(association, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapExistingAsync(association.Id, cancellationToken);
    }

    public async Task<AssociationDto> UpdateAsync(int currentUserId, UpdateAssociationDto dto, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, EditAssociationsPermissionId, cancellationToken);

        ValidateName(dto.EnglishName, "English association name");
        ValidateName(dto.ArabicName, "Arabic association name");

        var association = await _unitOfWork.Associations.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Association was not found.");

        await EnsureCityExistsAsync(dto.CityId, cancellationToken);

        association.EnglishName = dto.EnglishName.Trim();
        association.ArabicName = dto.ArabicName.Trim();
        association.CityId = dto.CityId;
        association.LocationOnGoogleMaps = string.IsNullOrWhiteSpace(dto.LocationOnGoogleMaps) ? null : dto.LocationOnGoogleMaps.Trim();
        association.Latitude = ParseCoordinate(dto.Latitude, "Latitude");
        association.Longitude = ParseCoordinate(dto.Longitude, "Longitude");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapExistingAsync(association.Id, cancellationToken);
    }

    // Phase 2a scope (MAYD-51 is a deliberately separate design discussion, not resolved here): this
    // is a PLAIN soft-delete of the Association record only. It does NOT cascade to Workers and does
    // NOT touch association-linked users — there is no AssociationUser entity yet (Phase 2b), and
    // whether deleting an association should also delete its workers/users at all is MAYD-51's own
    // open business-rule question, not something to guess an answer to in this pass. A deleted
    // Association's Workers rows are simply left as-is, still pointing at a now-soft-deleted
    // AssociationId — revisit once MAYD-51 is actually decided.
    public async Task DeleteAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, DeleteAssociationsPermissionId, cancellationToken);

        var association = await _unitOfWork.Associations.GetByIdAsync(associationId, cancellationToken)
            ?? throw new KeyNotFoundException("Association was not found.");

        // MaydanDbContext.SaveChangesAsync intercepts EntityState.Deleted for every SharedEntities
        // and converts it into a soft delete (IsDeleted = true, DeletedAt = UtcNow) — this does not
        // hard-delete the row (same pattern as ProjectService.DeleteAsync/LocationService.DeleteCountryAsync).
        _unitOfWork.Associations.Remove(association);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssociationDto> RestoreAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default)
    {
        // Restore uses the same permission as Delete (DeleteAssociations OR ManageAssociations) —
        // matches the real frontend exactly: associations-list.component.ts's own
        // canRestoreAssociation() is a direct alias of canDeleteAssociation(), not a separate check.
        await GetAuthorizedUserAsync(currentUserId, DeleteAssociationsPermissionId, cancellationToken);

        var association = await _unitOfWork.Associations.GetByIdIncludingDeletedAsync(associationId, cancellationToken)
            ?? throw new KeyNotFoundException("Association was not found.");

        if (!association.IsDeleted)
        {
            throw new InvalidOperationException("Association is not deleted.");
        }

        association.IsDeleted = false;
        association.DeletedAt = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapExistingAsync(association.Id, cancellationToken);
    }

    // Loads the caller (with the full Role.RolePermissions/UserPermissions/UserGroups graph —
    // GetWithPermissionsAsync, not the lighter GetByIdAsync/GetDetailsAsync, for the same reason
    // UserManagementService.GetUsersPagedAsync/UpdateUserStatusAsync both need it: GetEffectivePermissionIds
    // reads user.Role.RolePermissions, which isn't loaded otherwise) and checks it holds
    // requiredPermissionId OR ManageAssociations — every Association action in this class is gated
    // this way, reads included, since there is no anonymous/public consumer of this module (unlike
    // Phase 1's Locations, which needed AllowAnonymous for the public signup flow).
    // Re-fetches the full DTO (with City/Country/WorkersCount) right after Create/Update/Restore —
    // deliberately NOT routed through the public GetByIdAsync, which re-checks ViewAssociations.
    // That check already happened once, against the actual permission the write itself needed
    // (Create/Edit/Delete), at the top of the calling method — a caller who holds only
    // EditAssociations (not ViewAssociations) must still be able to see the result of their own
    // successful edit, not get a 403 immediately after it.
    private async Task<AssociationDto> MapExistingAsync(int associationId, CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.Associations.GetByIdWithWorkersCountAsync(associationId, cancellationToken)
            ?? throw new KeyNotFoundException("Association could not be loaded after the change.");

        return MapToDto(result.Association, result.WorkersCount);
    }

    private async Task<User> GetAuthorizedUserAsync(int currentUserId, int requiredPermissionId, CancellationToken cancellationToken)
    {
        var currentUser = await _unitOfWork.Users.GetWithPermissionsAsync(currentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Current user was not found.");

        if (!currentUser.IsActive)
        {
            throw new UnauthorizedAccessException("Current user is inactive.");
        }

        if (!GetEffectivePermissionIds(currentUser).Overlaps(new[] { requiredPermissionId, ManageAssociationsPermissionId }))
        {
            throw new UnauthorizedAccessException("Caller does not hold the required Associations permission.");
        }

        return currentUser;
    }

    // Same active-only lookup LocationService.CreateCityAsync/UpdateCityAsync already use to
    // validate a City FK — CityId is the only location field Association actually persists.
    private async Task EnsureCityExistsAsync(int cityId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Cities.GetByIdAsync(cityId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("City was not found.");
        }
    }

    private static void ValidateName(string value, string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldLabel} is required.");
        }

        if (value.Trim().Length > MaxNameLength)
        {
            throw new InvalidOperationException($"{fieldLabel} cannot exceed {MaxNameLength} characters.");
        }
    }

    // CreateAssociation/UpdateAssociation send latitude/longitude as plain strings (the frontend's
    // own Association model types them as string, built via lat.toFixed(6) — see
    // association-form.component.ts's setMapSelection). Optional: the form only requires them on
    // create (setLocationValidators(true) when there's no associationId yet), not on edit.
    private static decimal? ParseCoordinate(string? value, string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"{fieldLabel} is not a valid number.");
        }

        return parsed;
    }

    private static HashSet<int> GetEffectivePermissionIds(User user)
    {
        var rolePermissionIds = user.Role.RolePermissions
            .Where(rp => rp.IsActive && rp.Permission.IsActive)
            .Select(rp => rp.PermissionId);

        var directPermissionIds = user.UserPermissions
            .Where(up => up.IsActive && up.Permission.IsActive)
            .Select(up => up.PermissionId);

        var groupPermissionIds = user.UserGroups
            .Where(ug => ug.Group.IsActive)
            .SelectMany(ug => ug.Group.GroupPermissions)
            .Where(gp => gp.IsActive && gp.Permission.IsActive)
            .Select(gp => gp.PermissionId);

        return rolePermissionIds.Concat(directPermissionIds).Concat(groupPermissionIds).ToHashSet();
    }

    private static AssociationDto MapToDto(Association association, int workersCount) => new()
    {
        Id = association.Id,
        EnglishName = association.EnglishName,
        ArabicName = association.ArabicName,
        LocationOnGoogleMaps = association.LocationOnGoogleMaps ?? string.Empty,
        Latitude = FormatCoordinate(association.Latitude),
        Longitude = FormatCoordinate(association.Longitude),
        CountryId = association.City.CountryId,
        CountryEnglishName = association.City.Country.EnglishName,
        CountryArabicName = association.City.Country.ArabicName,
        CityId = association.CityId,
        CityEnglishName = association.City.EnglishName,
        CityArabicName = association.City.ArabicName,
        IsDeleted = association.IsDeleted,
        WorkersCount = workersCount
    };

    private static string FormatCoordinate(decimal? value) =>
        value?.ToString("0.######", CultureInfo.InvariantCulture) ?? string.Empty;
}

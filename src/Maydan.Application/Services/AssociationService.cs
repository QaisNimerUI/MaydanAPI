using System.Globalization;
using Maydan.Application.DTOs.Associations;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

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
// linking (Phase 2b — no AssociationUser entity exists yet; Phase 2c's own investigation confirmed
// "users associated with an association" are simply real User rows with EntityType.Association +
// EntityId == association.Id, Phase 2b's own real wiring), CityLocations (a later phase — Association
// has no CityLocationId column; see AssociationDto's own comment), and the Service-Request-based
// edit/delete restriction (MAYD-48/50 — no ServiceRequest module exists anywhere in this codebase
// yet, confirmed by grep).
//
// MAYD-51 (Association Management, Phase 2c, 2026-09-26): "Delete Associated Users & Workers" — see
// DeleteAsync/RestoreAsync's own comments for the cascade this phase adds.
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

    // MAYD-51 ("Delete Associated Users & Workers"): cascades the same soft-delete to every real
    // User/Worker row scoped to this Association — "users associated with an association" are real
    // User rows with EntityType.Association + EntityId == association.Id (Phase 2b's own real
    // wiring; no separate AssociationUser link entity exists), reusing GetByEntityAsync exactly as
    // UserManagementService.GetUsersAsync/CreateUserAsync already do; "workers associated with an
    // association" come from the SAME tracked Association.Workers collection loaded by
    // GetByIdWithWorkersAsync below (see that method's own comment on IAssociationRepository for
    // why — a required, Restrict-behavior Worker->Association relationship needs the collection
    // actually loaded before both sides can be marked deleted in the same SaveChanges call). Both
    // sources respect the global soft-delete filter (active-only, automatically for the Included
    // collection too), so every row here is guaranteed not already deleted — no extra IsDeleted
    // check needed before staging each Remove().
    //
    // The ticket's own explicit requirement ("existence of workers must not prevent deletion") was
    // already true before this phase — DeleteAsync never had an existence check blocking it; this
    // only ADDS the cascade, it doesn't remove a block that was never there.
    //
    // This does NOT re-check Manage Users/Manage Workers for the caller — deleting an association is
    // one authorized action (DeleteAssociations/ManageAssociations, checked once above), not a
    // proxy for independently re-authorizing bulk user/worker management.
    public async Task DeleteAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default)
    {
        await GetAuthorizedUserAsync(currentUserId, DeleteAssociationsPermissionId, cancellationToken);

        var association = await _unitOfWork.Associations.GetByIdWithWorkersAsync(associationId, cancellationToken)
            ?? throw new KeyNotFoundException("Association was not found.");

        var users = await _unitOfWork.Users.GetByEntityAsync(EntityType.Association, association.Id, search: null, cancellationToken);

        // Snapshot into a plain list before removing anything — Remove() below keeps the in-memory
        // graph consistent by also removing each Worker from this same Association.Workers
        // collection as it's marked deleted, which would otherwise invalidate a live foreach over
        // the collection itself.
        var workers = association.Workers.ToList();

        // MaydanDbContext.SaveChangesAsync intercepts EntityState.Deleted for every SharedEntities
        // and converts it into a soft delete (IsDeleted = true, DeletedAt = utcNow) — this does not
        // hard-delete the row (same pattern as ProjectService.DeleteAsync/LocationService.DeleteCountryAsync).
        // Confirmed: utcNow is computed ONCE per SaveChangesAsync call and applied to every entity
        // staged in it, not once per entity — so the Association and every cascaded User/Worker
        // below, all Removed before the single SaveChangesAsync call that follows, get the EXACT
        // SAME DeletedAt. RestoreAsync's own cascade relies on that same-instant equality to tell
        // "deleted in THIS cascade" apart from "deleted independently" (see its own comment) — no
        // separate timestamp needs to be threaded through by hand.
        //
        // Order matters here: the Workers (and, for consistency, Users) must be Removed BEFORE the
        // Association itself — confirmed live. Worker->Association is a required, Restrict-behavior
        // relationship (WorkerConfiguration.cs); marking the Association Deleted first, while its
        // (now-loaded) Workers collection still has entries that aren't ALSO marked Deleted yet,
        // makes EF Core's own cascade-behavior check throw ("the association between entity types
        // 'Association' and 'Worker' has been severed...") — even though every entity ends up
        // Removed before the single SaveChangesAsync call below. Deleting dependents first avoids it.
        foreach (var user in users)
        {
            _unitOfWork.Users.Remove(user);
        }

        foreach (var worker in workers)
        {
            _unitOfWork.Workers.Remove(worker);
        }

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

        // MAYD-51 symmetric restore: MAYD-51's own text only talks about delete, but leaving
        // DeleteAsync's cascade with no restore-side counterpart would permanently orphan a real
        // User/Worker the moment their Association is restored — clearly wrong. Blindly restoring
        // EVERY currently-deleted User/Worker under this AssociationId is ALSO wrong: one could have
        // been soft-deleted independently, for an unrelated reason, before or after the
        // Association's own deletion — reviving it just because the Association came back would be
        // a real, silent bug. The fix: only restore a User/Worker whose own DeletedAt exactly
        // matches THIS Association's own DeletedAt — i.e. it was deleted in the very same cascade
        // (see DeleteAsync's own comment on why every entity in one cascade shares one identical
        // DeletedAt instant). Captured BEFORE clearing the Association's own DeletedAt below, since
        // that's the value being compared against.
        var cascadeDeletedAt = association.DeletedAt;

        var deletedUsers = await _unitOfWork.Users.GetDeletedByEntityAsync(EntityType.Association, association.Id, cancellationToken);
        var deletedWorkers = await _unitOfWork.Workers.GetDeletedByAssociationIdAsync(association.Id, cancellationToken);

        association.IsDeleted = false;
        association.DeletedAt = null;

        // Restore isn't a Remove()/interceptor-driven transition (there's no "un-delete" interception
        // — clearing IsDeleted is a normal property update) — hand-set the same two fields the
        // interceptor itself owns on the way down, matching how the Association's own restore above
        // already does it, for exactly the rows whose DeletedAt matches the cascade being reversed.
        foreach (var user in deletedUsers.Where(u => u.DeletedAt == cascadeDeletedAt))
        {
            user.IsDeleted = false;
            user.DeletedAt = null;
        }

        foreach (var worker in deletedWorkers.Where(w => w.DeletedAt == cascadeDeletedAt))
        {
            worker.IsDeleted = false;
            worker.DeletedAt = null;
        }

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

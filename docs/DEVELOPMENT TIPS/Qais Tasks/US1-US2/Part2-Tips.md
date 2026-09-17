# User Management Module

## Part 2 - Entity User Management APIs

### Status

Implemented - first backend API pass.

This part starts the API development for the User Management module after Part 1 completed the domain model, EF Core configuration, password hashing, migrations, and database update.

The page name in the business story is **Permissions**, not Roles. The Permissions page can still display role-based permission data as a read-only permission matrix.

Current implementation uses `X-Current-User-Id` as a Swagger/dev fallback when no JWT user id claim is available. Replace this with authorization policies once the auth module and permissions claims are ready.

Implemented files:

- `src/Maydan.Application/DTOs/Auth/AuthDtos.cs`
- `src/Maydan.Application/Interfaces/IAuthService.cs`
- `src/Maydan.Application/Interfaces/IJwtTokenGenerator.cs`
- `src/Maydan.Application/Services/AuthService.cs`
- `src/Maydan.API/Controllers/AuthController.cs`
- `src/Maydan.API/Security/JwtTokenGenerator.cs`
- `src/Maydan.Application/DTOs/UserManagement/UserManagementDtos.cs`
- `src/Maydan.Application/Interfaces/IUserManagementService.cs`
- `src/Maydan.Application/Services/UserManagementService.cs`
- `src/Maydan.API/Controllers/UsersController.cs`
- `src/Maydan.API/Controllers/PermissionsController.cs`
- `src/Maydan.API/Controllers/GroupsController.cs`
- `src/Maydan.API/Controllers/ApiControllerBase.cs`
- Repository query methods for Users, Permissions, Groups, and Roles.
- DI registration in `src/Maydan.API/Program.cs`.
- `MaydanDbContext.SaveChangesAsync` keeps `UserGroup` deletes physical because `UserGroup` is a join table with a composite key.

Verification:

- `dotnet build Maydan.sln --no-restore` passes.
- `dotnet test Maydan.sln --no-restore --no-build` passes.
- `dotnet ef migrations has-pending-model-changes --project src/Maydan.Infrastructure --startup-project src/Maydan.API` reports no pending model changes.

---

# Swagger API Naming

The API routes were renamed from generic REST-style names to business-action names so Swagger is easier to read for QA and business users.

Logic structure did not change. Controllers still call the same `IUserManagementService` methods, and the same repository/UnitOfWork flow is used.

| Swagger/API name | Current route | Replaces old route |
| --- | --- | --- |
| Login User | `POST /api/auth/login` | New auth endpoint |
| Get Current Entity Users | `GET /api/users/current-entity` | `GET /api/users` |
| Search Current Entity Users | `GET /api/users/search?search={term}` | `GET /api/users?search={term}` |
| Get User Details | `GET /api/users/{userId}/details` | `GET /api/users/{userId}` |
| Register User | `POST /api/users/register` | `POST /api/users` |
| Update User Direct Permissions | `PUT /api/users/{userId}/direct-permissions/update` | `PUT /api/users/{userId}/permissions` |
| Update User Groups | `PUT /api/users/{userId}/groups/update` | `PUT /api/users/{userId}/groups` |
| Get User Effective Permissions | `GET /api/users/{userId}/effective-permissions` | `GET /api/users/{userId}/effective-permissions` |
| Get Available Permissions | `GET /api/permissions/available?roleId={roleId}` | `GET /api/permissions/available?roleId={roleId}` |
| Get Permission Matrix | `GET /api/permissions/matrix` | `GET /api/permissions/matrix` |
| Get Current Entity Groups | `GET /api/groups/current-entity` | `GET /api/groups` |
| Get Group Details | `GET /api/groups/{groupId}/details` | `GET /api/groups/{groupId}` |
| Create Permission Group | `POST /api/groups/create` | `POST /api/groups` |
| Update Permission Group | `PUT /api/groups/{groupId}/update` | `PUT /api/groups/{groupId}` |
| Delete Permission Group | `DELETE /api/groups/{groupId}/delete` | `DELETE /api/groups/{groupId}` |

## Login User Swagger Body

Seed admin login:

```json
{
  "email": "ghaith@baytalurdon.org",
  "password": "Abc@123"
}
```

Expected result for users with `MustResetPassword = false`:

- `isAuthenticated = true`
- `mustResetPassword = false`
- `accessToken` is returned
- UI should use the token as `Authorization: Bearer {token}`

Expected result for users with `MustResetPassword = true`:

- `isAuthenticated = false`
- `mustResetPassword = true`
- `accessToken = null`
- UI should redirect the user to the password reset flow before allowing system access.

Invalid credentials return `401 Unauthorized`.

---

# 1. Goal

Build backend APIs that allow an Entity Admin to manage users, permissions, groups, and user access inside their assigned entity only.

The API order is:

1. Users
2. Permissions
3. Groups
4. User Access

---

# 2. Business Scope

Entity Admins can manage only data inside their own entity.

Entity scope is determined by:

- `User.EntityType`
- `User.EntityId`

This rule applies to:

- Users
- Groups
- Direct user permissions
- User group assignments
- Effective permissions

No API in this part should allow an Entity Admin to manage users or groups belonging to another entity.

---

# 3. Permission Rules

Required permissions should be validated by backend logic.

Initial permission examples:

- `ViewUsers`
- `CreateUser`
- `EditUser`
- `ViewPermissions`
- `AssignPermissions`
- `ViewGroups`
- `CreateGroup`
- `EditGroup`
- `DeleteGroup`
- `AssignGroups`

RolePermissions define which permissions are available for a user's Role.

UserPermissions define what the specific user actually has.

GroupPermissions define reusable permission packages.

Effective permissions are:

`UserPermissions + UserGroups -> GroupPermissions`

Duplicate permissions should be returned once.

---

# 4. Phase 2.1 - Users APIs

## 4.1 Get users for current entity

Create an API that returns users belonging to the current admin's entity.

Expected behavior:

- Uses current authenticated user.
- Reads `currentUser.EntityType` and `currentUser.EntityId`.
- Returns only users with the same entity scope.
- Excludes soft-deleted users.

Example endpoint:

`GET /api/users`

or:

`GET /api/entity-users`

## 4.2 Search users

Support search by:

- Username / full name
- Email
- Mobile number

Search must still stay inside the current entity scope.

Example:

`GET /api/users?search=ahmad`

## 4.3 Get user details

Return one user with:

- User details
- Role
- Direct permissions
- Assigned groups
- Effective permissions

Scope rule:

The target user must belong to the current admin's entity.

Example:

`GET /api/users/{userId}`

## 4.4 Create user

Create a user under the current admin's entity.

The frontend will use a stepper:

1. User details
2. Select permissions
3. Select groups

Backend can receive this as one request.

Request should include:

- FirstNameAr
- FirstNameEn
- LastNameAr
- LastNameEn
- PhoneNumber
- Email
- InitialPassword
- RoleId
- SelectedPermissionIds
- SelectedGroupIds

Rules:

- Email must be unique across the system.
- Password must be hashed using `IPasswordHasher`.
- User is linked automatically to current admin's `EntityType` and `EntityId`.
- `SelectedPermissionIds` is optional.
- `SelectedGroupIds` is optional.
- Admin can create a user without permissions or groups.
- Selected permissions must be inside RolePermissions for the selected Role.
- Selected groups must belong to the current admin's entity.

Example:

`POST /api/users`

---

# 5. Phase 2.2 - Permissions APIs

## 5.1 Get available permissions

Return permissions available to assign for a selected Role.

Expected behavior:

- Loads permissions from RolePermissions.
- Filters out permissions not applicable to the current admin/entity.
- Used by user creation and edit flows.

Example:

`GET /api/permissions/available?roleId=4`

## 5.2 Get permission matrix

Return a read-only matrix for the Permissions page.

The matrix should display:

- Role names in Arabic and English
- Permission names in Arabic and English
- Whether each permission is available for each role

Rules:

- Entity Admin can view only applicable roles/permissions.
- Entity Admin cannot create roles.
- Entity Admin cannot edit roles.
- Entity Admin cannot delete roles.

Example:

`GET /api/permissions/matrix`

---

# 6. Phase 2.3 - Groups APIs

Groups are entity-scoped.

Group uniqueness is already handled by composite indexes:

- `(EntityType, EntityId, GroupNameEn)`
- `(EntityType, EntityId, GroupNameAr)`

## 6.1 Get groups

Return groups belonging to the current admin's entity.

Example:

`GET /api/groups`

## 6.2 Get group details

Return:

- Group details
- Group permissions
- Assigned users

Example:

`GET /api/groups/{groupId}`

## 6.3 Create group

Create a custom permission group inside the current admin's entity.

Request should include:

- GroupNameAr
- GroupNameEn
- PermissionIds
- UserIds

Rules:

- Group is linked automatically to current admin's entity.
- Group name cannot duplicate another group name inside the same entity.
- Selected permissions must be valid for the current entity/admin scope.
- Selected users must belong to the current admin's entity.

Example:

`POST /api/groups`

## 6.4 Update group

Allow editing:

- GroupNameAr
- GroupNameEn
- Permissions
- Assigned users

Rules:

- Group must belong to current admin's entity.
- New names must remain unique inside that entity.
- Selected permissions and users must stay within entity scope.

Example:

`PUT /api/groups/{groupId}`

## 6.5 Delete group

Delete a group belonging to the current admin's entity.

Important business rule:

When a group assigned to users is deleted, all permissions associated with that group shall remain assigned to the affected users as individual permissions.

Delete flow:

1. Load group with GroupPermissions and UserGroups.
2. For each assigned user, copy the group's permissions into UserPermissions.
3. Do not duplicate existing UserPermissions.
4. Remove or soft-delete UserGroup links.
5. Remove or soft-delete GroupPermission links.
6. Soft-delete the Group.

Example:

`DELETE /api/groups/{groupId}`

---

# 7. Phase 2.4 - User Access APIs

## 7.1 Update direct user permissions

Allow Entity Admin to replace or update a user's direct permissions.

Rules:

- Target user must belong to current admin's entity.
- Permissions must be allowed by target user's RolePermissions.
- Admin must have `AssignPermissions`.
- A user should not modify their own permissions.

Example:

`PUT /api/users/{userId}/permissions`

## 7.2 Assign/remove groups

Allow assigning and removing groups from a user.

Rules:

- Target user must belong to current admin's entity.
- Groups must belong to current admin's entity.
- Admin must have `AssignGroups`.
- Re-adding a soft-deleted UserGroup should restore the existing relationship if needed.

Example:

`PUT /api/users/{userId}/groups`

## 7.3 Get effective permissions

Return the final effective permissions for a user.

Effective permissions:

`Direct UserPermissions + GroupPermissions inherited from UserGroups`

Rules:

- Target user must belong to current admin's entity.
- Duplicate permissions return once.

Example:

`GET /api/users/{userId}/effective-permissions`

---

# 8. Phase 2.5 - Authorization and Scope Enforcement

Every API in this part must validate:

- Current user is authenticated.
- Current user is active.
- Current user has the required permission.
- Target users belong to the current user's entity.
- Target groups belong to the current user's entity.
- Assigned permissions are valid for the target role.

Do not rely only on frontend validation.

---

# 9. Phase 2.6 - Testing

## Users tests

- Entity Admin gets only users from their entity.
- Search works by name, email, and mobile number.
- User details cannot be loaded from another entity.
- User can be created without permissions or groups.
- Duplicate email is rejected.
- Initial password is hashed.

## Permissions tests

- Available permissions are loaded by RolePermissions.
- Permission matrix is read-only.
- Entity Admin cannot create, edit, or delete roles from the Permissions page.

## Groups tests

- Group names are unique inside the same entity.
- Same group name is allowed across different entities.
- Group cannot include permissions outside allowed scope.
- Group cannot be assigned to users from another entity.
- Deleting a group preserves its permissions as direct UserPermissions for affected users.

## User Access tests

- Direct permissions can be assigned.
- Groups can be assigned and removed.
- Effective permissions include direct and group permissions.
- Duplicate effective permissions are returned once.
- User cannot modify their own permissions.

---

# 10. Recommended Development Order

1. Add/complete repository methods needed for Users, Permissions, Groups, UserGroups, and UserPermissions.
2. Add DTOs for user list, user details, create user, permissions matrix, groups, and user access.
3. Add service layer for entity-scoped user management.
4. Implement Users APIs.
5. Implement Permissions APIs.
6. Implement Groups APIs.
7. Implement User Access APIs.
8. Add authorization/scope checks.
9. Add focused tests.

---

# 11. Definition of Done

Part 2 is complete when:

- Users APIs exist and enforce current entity scope.
- User search works by name, email, and phone number.
- Create user supports optional permissions and groups.
- Permissions page APIs return available permissions and a read-only matrix.
- Groups APIs support create, edit, delete, list, and details.
- Deleting a group preserves group permissions as direct user permissions.
- User Access APIs support direct permissions, group assignment, and effective permissions.
- Arabic and English names are available in API responses where needed.
- Build passes.
- Tests cover the main business rules.

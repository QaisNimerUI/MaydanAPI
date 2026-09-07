# User Management Module - Implementation Plan

## Core Concept

The User Management module is based on three main concepts:

- Users
- Roles
- Permissions

Roles identify the user's entity/type:

- Bayt AlUrdon
- ASEZA
- Association
- Production House

A Role does not automatically give the user full permissions.

The actual access of each user is controlled through UserPermissions.

Example:

Ghaith:
- Role: Bayt AlUrdon
- Permissions: Full set of allowed permissions

Aya:
- Role: Bayt AlUrdon
- Permissions: Selected permissions only

Both users belong to the same Role, but their access levels are different.

---

# Part 1 - User Management Database Design

## Phase 1.1 - Role

Create the Role entity.

Fields:

- Id
- Name

Seeded Roles:

- Bayt AlUrdon
- ASEZA
- Association
- Production House

Purpose:

The Role identifies which entity/type the user belongs to.

---

## Phase 1.2 - Permission

Create the Permission entity.

Fields:

- Id
- Name
- Code
- Description (Optional)

Examples:

- ViewUsers
- CreateUser
- EditUser
- DeleteUser
- AssignPermissions
- ViewAssociations
- CreateAssociation
- EditAssociation
- ViewProductionHouses

Permissions will be seeded based on the permissions provided by the business.

---

## Phase 1.3 - RolePermissions

Create the RolePermission entity.

Fields:

- Id
- RoleId
- PermissionId

Relationship:

Role
1 -> Many RolePermissions

Permission
1 -> Many RolePermissions

Purpose:

Defines which permissions are available for each Role.

Example:

Association Role may support:

- ViewUsers
- CreateUser
- EditUser
- ViewAssociation

Production House Role may support:

- ViewUsers
- CreateUser
- ViewProductionHouse

A user cannot receive a permission that is not available for their Role.

Add Unique Constraint:

(RoleId, PermissionId)

---

## Phase 1.4 - User

Create/complete the User entity.

Fields:

- Id
- Email
- PasswordHash
- RoleId
- EntityType
- EntityId
- MustResetPassword
- IsActive
- CreatedAt
- UpdatedAt

Important Rules:

- Email must be unique across the whole system.
- Every User belongs to one Role.
- Every User must be linked to an Entity when applicable.
- Password must never be stored as plain text.
- Passwords are hashed through `IPasswordHasher` / `PasswordHasher`.
- Current hashing format: `PBKDF2-SHA256.{iterations}.{salt}.{hash}`.

---

## Phase 1.5 - UserPermissions

Create the UserPermission entity.

Fields:

- Id
- UserId
- PermissionId

Purpose:

Stores the actual permissions granted to an individual user.

Example:

Ghaith:

- CreateUser
- EditUser
- DeleteUser
- AssignPermissions
- CreateAssociation
- ...

Aya:

- ViewUsers
- EditUser
- ViewAssociations

Even though both users have:

Role = Bayt AlUrdon

their actual access is different.

Add Unique Constraint:

(UserId, PermissionId)

---

# Part 2 - EF Core Configuration

## Phase 2.1 - Entity Configurations

Create EF Core configurations for:

- User
- Role
- Permission
- RolePermission
- UserPermission

Configure:

- Primary Keys
- Foreign Keys
- Relationships
- Max lengths
- Required fields
- Unique Email
- Unique RolePermission
- Unique UserPermission

---

## Phase 2.2 - DbContext

Add DbSets:

- Users
- Roles
- Permissions
- RolePermissions
- UserPermissions

---

## Phase 2.3 - Seed Data

Seed:

### Roles

- Bayt AlUrdon
- ASEZA
- Association
- Production House

### Permissions

Seed all permissions defined by the business.

### RolePermissions

Seed which permissions are available for each Role.

Initial Bayt AlUrdon system user may receive all available permissions through UserPermissions.

---

## Phase 2.4 - First Migration

Because the project is Code First and currently has no database:

Create the initial migration after the entities and configurations are finalized.

Then create the database using:

Update-Database

Do not create the migration before the User Management schema is reviewed and finalized.

---

# Part 3 - Create User Flow

## Phase 3.1 - Create User Request

Create a request containing:

- Email
- InitialPassword
- RoleId
- EntityId
- SelectedPermissionIds

The EntityType may be derived from the selected Role depending on the final implementation.

---

## Phase 3.2 - Create User Business Logic

When creating a User:

1. Validate that the email is unique.
2. Validate the Role.
3. Validate the target Entity.
4. Validate that the selected permissions belong to the RolePermissions of the selected Role.
5. Hash the Initial Password.
6. Create the User.
7. Link the User to the Entity.
8. Save the selected UserPermissions.
9. Set MustResetPassword according to the business rules.

---

# Part 4 - Users Under Each Entity

## Phase 4.1 - Get Entity Users

Create an API to retrieve Users belonging to a specific Entity.

Example:

GET /api/entities/{entityType}/{entityId}/users

Or separate endpoints can be used depending on the existing project structure.

Examples:

GET /api/associations/{associationId}/users

GET /api/production-houses/{productionHouseId}/users

---

## Phase 4.2 - Create Entity User

Create an endpoint to create a User under an Entity.

Example:

POST /api/entities/{entityType}/{entityId}/users

The created User must automatically be linked to that Entity.

---

## Phase 4.3 - Manage Entity Users

Depending on the final requirements, support:

- View User
- Edit User
- Activate/Deactivate User
- Update User Permissions

Avoid hard delete unless explicitly required by the business.

---

# Part 5 - Permission Management

## Phase 5.1 - Load Available Permissions

When creating or editing a User:

The system loads the RolePermissions of the user's Role.

Example:

Role = Association

The UI receives only the permissions available for Association.

---

## Phase 5.2 - Assign User Permissions

The creator selects which permissions the new User receives.

Selected permissions are stored in UserPermissions.

RolePermissions define:

"What can this Role potentially have?"

UserPermissions define:

"What does this specific User actually have?"

---

## Phase 5.3 - AssignPermissions Permission

Add a permission such as:

AssignPermissions

A User with this permission may assign or modify permissions for other Users within their allowed scope.

---

## Phase 5.4 - Prevent Self Permission Modification

Business Rule:

A User cannot modify their own permissions.

Even if the User has:

AssignPermissions

the system must reject attempts to:

- Add permissions to themselves
- Remove permissions from themselves
- Change their own permission set

This is a system/business rule, not another Permission.

---

## Phase 5.5 - Prevent Permission Escalation

A User must not be able to assign a permission that is outside the target Role's RolePermissions.

The backend must always validate this.

Do not depend only on the frontend.

---

# Part 6 - Authentication and Initial Password

## Phase 6.1 - Login

Create Login functionality using:

- Email
- Password

Validate:

- User exists
- User is active
- Password is correct

Password verification must use:

`IPasswordHasher.VerifyPassword(password, user.PasswordHash)`

---

## Phase 6.2 - Initial Password Rule

For users that require password reset:

MustResetPassword = true

The Initial Password is temporary.

The User must not receive normal system access until the password is changed.

---

## Phase 6.3 - Reset Initial Password

Create an endpoint such as:

POST /api/auth/reset-initial-password

The flow:

1. Validate User.
2. Validate current/temporary password if required.
3. Validate new password.
4. Hash new password using `IPasswordHasher.HashPassword(newPassword)`.
5. Replace PasswordHash.
6. Set:

MustResetPassword = false

7. Allow normal login afterwards.

---

## Phase 6.4 - Production House Exception

According to the business rule:

Production House users created through the required flow may be allowed to access the system without mandatory first-login password reset.

For those users:

MustResetPassword = false

The exact creation scenario should be enforced according to the final business requirement.

---

# Part 7 - Authorization

## Phase 7.1 - Permission Based Authorization

Authorization must depend primarily on UserPermissions.

Example:

Creating a User requires:

CreateUser

Assigning permissions requires:

AssignPermissions

Creating an Association requires:

CreateAssociation

---

## Phase 7.2 - Entity Scope Validation

Permissions alone are not enough.

The backend must also validate the Entity scope.

Example:

A User belonging to Association A must not automatically manage Users belonging to Association B.

Authorization therefore depends on:

User
+
Role
+
UserPermissions
+
Entity Scope

---

# Part 8 - Association Management

## Phase 8.1 - Create Association

Implement Association creation.

Required permission example:

CreateAssociation

The business determines which Roles have this permission available through RolePermissions.

Do not hardcode logic such as:

if Role == SuperAdmin

Instead use permissions and scope.

---

# Part 9 - Production House Self Registration

## Phase 9.1 - Registration Endpoint

Create a Production House registration flow.

Example:

POST /api/auth/register-production-house

---

## Phase 9.2 - Registration Logic

The flow should:

1. Validate registration information.
2. Create the Production House Entity.
3. Create its initial User.
4. Assign Role = Production House.
5. Link User.EntityId to the created Production House.
6. Assign the required initial UserPermissions.
7. Hash the password.
8. Set MustResetPassword according to the Production House business rule.

---

# Part 10 - Validation and Testing

## Phase 10.1 - User Creation Tests

Test:

- Duplicate email is rejected.
- User is linked to correct Entity.
- Password is hashed.
- UserPermissions are stored correctly.
- Invalid Role permission cannot be assigned.

---

## Phase 10.2 - Permission Tests

Test:

- Two Users with the same Role can have different permissions.
- User without CreateUser cannot create Users.
- User without AssignPermissions cannot change permissions.
- User cannot modify their own permissions.
- User cannot assign permissions outside the target RolePermissions.

---

## Phase 10.3 - Entity Scope Tests

Test:

- Users can manage only Entities allowed by their scope.
- Association User cannot manage another Association unless explicitly permitted by the business.
- Production House User cannot manage unrelated Entities.

---

## Phase 10.4 - Password Tests

Test:

- Password hash does not contain the plain text password.
- Correct password verifies against its hash.
- Incorrect password does not verify against the hash.
- Invalid hash format returns false.
- Initial password works only for the required reset flow.
- User with MustResetPassword = true cannot access protected system functionality normally.
- Password reset changes MustResetPassword to false.
- Production House exception works according to the business rule.

---

# Recommended Implementation Order

1. Domain Entities
2. EF Core Configurations
3. Seed Roles
4. Seed Permissions
5. Seed RolePermissions
6. DbContext
7. Initial Migration
8. Create Database
9. User Creation Use Case
10. Entity Users APIs
11. User Permission Management
12. Authentication
13. Initial Password Reset
14. Permission-Based Authorization
15. Entity Scope Authorization
16. Association Management
17. Production House Registration
18. Testing

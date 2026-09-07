# User Management Module

## Part 1 - Domain Model & Database Structure

### Status

Completed.

This part defines the core domain model and EF Core database structure required for the User Management module.

The project uses **EF Core Code First**, and the database has not been created yet.

Seed Data will be handled in the next part.

---

# 1. Core User Management Concept

The User Management module is based on:

- Users
- Roles
- Permissions
- Groups

Roles identify the user's entity/type.

Examples:

- Bayt AlUrdon
- ASEZA
- Association
- Production House

A Role does **not** automatically determine the user's actual system access.

For example, two users can both have:

Role = Bayt AlUrdon

while having completely different permissions.

The actual access of a user is determined through:

- Direct User Permissions
- Group Permissions

RolePermissions define which permissions are available for a specific Role.

---

# 2. User Entity

The `User` entity represents a system user.

Main fields:

- UserId
- FirstNameAr
- FirstNameEn
- LastNameAr
- LastNameEn
- PhoneNumber
- Email
- PasswordHash
- MustResetPassword
- IsActive
- RoleId
- EntityType
- EntityId

Relationships:

- User belongs to one Role.
- User can have multiple direct Permissions through UserPermission.
- User can belong to multiple Groups through UserGroup.

The User is also linked to a real-world entity using:

- EntityType
- EntityId

`EntityId` is treated as a soft reference because its target table depends on `EntityType`.

Examples:

- Association
- Production Company
- ASEZA
- Bayt AlUrdon

### User Business Rules

- Email is required.
- Email must be unique across the system.
- Password is stored as PasswordHash.
- Password hashing is implemented through `IPasswordHasher` in Application and `PasswordHasher` in Infrastructure.
- Current hashing format: `PBKDF2-SHA256.{iterations}.{salt}.{hash}`.
- MustResetPassword determines whether the user must change the initial password.
- IsActive determines whether the account can be used.
- RoleId determines the user's Role.
- EntityType + EntityId determine which entity the user belongs to.

---

# 3. Role Entity

The `Role` entity identifies the user's organization/entity type.

Fields:

- RoleId
- RoleNameAr
- RoleNameEn

Relationships:

- Role has many Users.
- Role has many Permissions through RolePermission.

Role does not automatically grant all available permissions to a User.

RolePermissions define the set of permissions that are available for users belonging to that Role.

---

# 4. Permission Entity

The `Permission` entity represents an individual system capability.

Fields:

- PermissionId
- PermissionNameEn
- PermissionNameAr
- Module

Examples of future permissions may include:

- ViewUsers
- CreateUser
- EditUser
- AssignPermissions

The final permission list will be defined through Seed Data according to the business requirements.

Relationships:

- Permission can belong to multiple Roles.
- Permission can belong to multiple Groups.
- Permission can be directly assigned to multiple Users.

---

# 5. Group Entity

The `Group` entity represents a reusable collection of Permissions.

Fields:

- GroupId
- GroupNameEn
- GroupNameAr

Relationships:

- Group can contain multiple Permissions through GroupPermission.
- Group can contain multiple Users through UserGroup.

Groups allow predefined permission packages to be assigned to Users.

Example:

Read Only Group

could contain:

- ViewUsers
- ViewAssociations
- ViewReports

Instead of assigning these permissions individually, the User can be added to the Group.

---

# 6. RolePermission

`RolePermission` represents the many-to-many relationship between:

Role <-> Permission

Fields:

- RoleId
- PermissionId

Composite Primary Key:

(RoleId, PermissionId)

Purpose:

Defines which Permissions are available for a specific Role.

Example:

Role = Association

Available Permissions:

- ViewUsers
- CreateUser
- EditUser
- ViewAssociation

A Permission outside the RolePermissions of a Role should not be assigned to a User belonging to that Role.

---

# 7. UserPermission

`UserPermission` represents the many-to-many relationship between:

User <-> Permission

Fields:

- UserId
- PermissionId

Composite Primary Key:

(UserId, PermissionId)

Purpose:

Defines Permissions directly assigned to an individual User.

Example:

Two Users can both have:

Role = Bayt AlUrdon

but:

User A:

- ViewUsers
- CreateUser
- EditUser
- AssignPermissions

User B:

- ViewUsers
- EditUser

Therefore, users with the same Role can have different system access.

---

# 8. UserGroup

`UserGroup` represents the many-to-many relationship between:

User <-> Group

Fields:

- UserId
- GroupId

Composite Primary Key:

(UserId, GroupId)

Purpose:

Allows a User to belong to one or more predefined permission Groups.

---

# 9. GroupPermission

`GroupPermission` represents the many-to-many relationship between:

Group <-> Permission

Fields:

- GroupId
- PermissionId

Composite Primary Key:

(GroupId, PermissionId)

Purpose:

Defines which Permissions are included inside a Group.

---

# 10. Effective User Permissions

# Effective User Permissions

A User can receive permissions from multiple sources:

1. **Direct UserPermissions**
   - Permissions individually selected and assigned directly to the User.

2. **Group Permissions**
   - The User can be assigned to one or more Groups.
   - Each Group contains a predefined set of Permissions.

3. **Groups + Additional Direct Permissions**
   - A User can be assigned to a Group and also receive additional individual Permissions.
   - The Group provides the predefined permission set, while UserPermissions provide additional permissions specific to that User.

The final effective permissions are the union of:

Effective Permissions
=
Direct UserPermissions
+
Permissions inherited from UserGroups -> GroupPermissions

Example:

User: Aya

Selected Group:
HR Standard
- View Workers
- View Attendance
- View Reports

Additional Direct Permissions:
- Create Worker
- Edit Worker

Aya's Effective Permissions:
- View Workers
- View Attendance
- View Reports
- Create Worker
- Edit Worker

If the same Permission exists both directly and through a Group, it should be treated as one effective Permission.

All assigned permissions, whether direct or inherited through Groups, must remain within the permissions allowed for the User's Role through RolePermissions.

Conceptually:

Effective User Permissions
=
Direct UserPermissions
+
Permissions from UserGroups -> GroupPermissions

Duplicate permissions should be treated as a single effective permission.

RolePermissions act as the allowed permission boundary for the Role.

---

# 11. Permission Management Business Rules

The following rules were established:

### Same Role, Different Permissions

Users belonging to the same Role do not necessarily have the same permissions.

Example:

Ghaith:

Role:
Bayt AlUrdon

Permissions:
Full allowed permission set.

Aya:

Role:
Bayt AlUrdon

Permissions:
Selected permissions only.

---

### Assign Permissions

A permission such as:

AssignPermissions

will determine whether a User can manage Permissions for other Users.

---

### Self Permission Modification

A User cannot modify their own Permissions.

Even if the User has:

AssignPermissions

the User must not be allowed to:

- Add permissions to themselves.
- Remove permissions from themselves.
- Modify their own permission set.

This is a fixed business/system rule and is not represented as another Permission.

---

### Permission Boundary

A User must not be assigned a Permission that is not available through the target Role's RolePermissions.

This rule must eventually be validated by the backend and must not depend only on frontend validation.

---

# 12. SharedEntities

The following entities inherit from `SharedEntities`:

- User
- Role
- Permission
- Group
- UserPermission
- RolePermission
- UserGroup
- GroupPermission

Therefore, they participate in the project's shared entity behavior, including the fields and functionality provided by `SharedEntities`.

This includes the existing:

- CreatedAt
- UpdatedAt
- IsDeleted
- DeletedAt

behavior.

The DbContext automatically manages CreatedAt and UpdatedAt.

Deleted SharedEntities are handled using Soft Delete.

---

# 13. Soft Delete

`MaydanDbContext` applies a global query filter to all entities inheriting from `SharedEntities`.

Normal queries automatically exclude:

IsDeleted = true

When an entity inheriting from SharedEntities is deleted through EF Core:

- The entity is not physically deleted.
- Its state is changed to Modified.
- IsDeleted is set to true.
- DeletedAt is populated.

---

# 14. Junction Table Soft Delete Consideration

The junction entities also inherit from SharedEntities:

- UserPermission
- RolePermission
- UserGroup
- GroupPermission

Their relationships use Composite Primary Keys.

Example:

UserPermission:

(UserId, PermissionId)

Because Soft Delete keeps the database record physically present, re-adding the exact same relationship later cannot simply create another row with the same Composite Primary Key.

Example:

UserId = 10
PermissionId = 5
IsDeleted = true

If Permission 5 is assigned to User 10 again, the application should restore the existing relationship instead of inserting a duplicate row.

This behavior must be handled later in the Application/Service layer.

The same rule applies to:

- RolePermission
- UserPermission
- UserGroup
- GroupPermission

---

# 15. EF Core Configurations

EF Core configurations were created/reviewed for:

- UserConfiguration
- RoleConfiguration
- PermissionConfiguration
- GroupConfiguration
- UserPermissionConfiguration
- RolePermissionConfiguration
- UserGroupConfiguration
- GroupPermissionConfiguration

Configurations include:

- Table names
- Primary Keys
- Composite Primary Keys
- Required properties
- Maximum lengths
- Unique indexes
- Foreign Keys
- Delete behaviors
- Entity relationships

---

# 16. Important Unique Constraints

The following uniqueness rules are configured or represented through the schema.

User:

Email must be unique.

Role:

RoleNameAr is unique.
RoleNameEn is unique.

Permission:

PermissionNameAr is unique.
PermissionNameEn is unique.

Group:

Group names are configured according to the current naming requirements.

Junction entities use Composite Primary Keys, preventing duplicate active database relationships with the same key combination:

RolePermission:
(RoleId, PermissionId)

UserPermission:
(UserId, PermissionId)

UserGroup:
(UserId, GroupId)

GroupPermission:
(GroupId, PermissionId)

---

# 17. DbContext

`MaydanDbContext` contains DbSets for the User Management module:

- Users
- Roles
- Permissions
- Groups
- RolePermissions
- UserPermissions
- UserGroups
- GroupPermissions

Configurations are automatically discovered using:

modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaydanDbContext).Assembly);

Therefore, individual configurations do not need to be manually registered inside OnModelCreating.

---

# 18. Current Relationship Model

User Management relationships:

Role
  |
  | 1:N
  |
User

Role
  |
  +---- RolePermission ---- Permission

User
  |
  +---- UserPermission ---- Permission

User
  |
  +---- UserGroup ---- Group
                         |
                         +---- GroupPermission ---- Permission

Conceptually:

RolePermissions
    =
Permissions available for a Role

UserPermissions
    =
Permissions directly assigned to a specific User

GroupPermissions
    =
Permissions included in a reusable Group

UserGroups
    =
Groups assigned to a specific User

---

# 19. Part 1 Status

Completed:

- User entity
- Role entity
- Permission entity
- Group entity
- UserPermission entity
- RolePermission entity
- UserGroup entity
- GroupPermission entity

Completed EF Core configurations:

- UserConfiguration
- RoleConfiguration
- PermissionConfiguration
- GroupConfiguration
- UserPermissionConfiguration
- RolePermissionConfiguration
- UserGroupConfiguration
- GroupPermissionConfiguration

Completed DbContext integration.

Completed SharedEntities integration.

Completed Soft Delete integration.

Completed Password Hashing integration:

- `IPasswordHasher` was added under `Maydan.Application.Interfaces`.
- `PasswordHasher` was added under `Maydan.Infrastructure.Security`.
- The implementation uses PBKDF2-SHA256 with a random salt and 100,000 iterations.
- `IPasswordHasher` is registered in API dependency injection.
- The initial seed user password `Abc@123` is stored as a generated hash in `UserSeedConfiguration`.
- Password hashing tests were added under `Maydan.Application.Tests`.

Database migration has not been created yet.

---

# Next Step - Part 2

The next implementation step is:

Seed Data

This will include defining and seeding:

1. Roles
2. Permissions
3. RolePermissions
4. Groups
5. GroupPermissions
6. Initial system User/UserPermissions if required

After the Seed Data structure is finalized, the Code First migration/database creation flow can continue.

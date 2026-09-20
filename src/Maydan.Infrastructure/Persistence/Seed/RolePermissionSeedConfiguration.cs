using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maydan.Infrastructure.Persistence.Seed
{
    // Grants each of the 4 real seeded roles (RoleSeedConfiguration) a subset of the permissions
    // seeded by PermissionSeedConfiguration, per the approved permission-matrix proposal.
    public class RolePermissionSeedConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        private const int BaytAlUrdon = 1;
        private const int Aseza = 2;
        private const int ProductionHouse = 3;
        private const int Association = 4;

        // Permission ids, matching PermissionSeedConfiguration.
        private const int ViewUsers = 1;
        private const int CreateUsers = 2;
        private const int ManageUsers = 3;
        private const int ViewGroups = 4;
        private const int CreateGroups = 5;
        private const int EditGroups = 6;
        private const int DeleteGroups = 7;
        private const int ManageGroups = 8;
        private const int ViewAssociations = 9;
        private const int CreateAssociations = 10;
        private const int EditAssociations = 11;
        private const int DeleteAssociations = 12;
        private const int ManageAssociations = 13;
        private const int ViewAssociationUsers = 14;
        private const int RequestService = 15;
        private const int ViewServiceRequests = 16;
        private const int ManageServiceRequests = 17;
        private const int ManageServices = 18;
        private const int ViewProductionCompanies = 19;
        private const int ManageProductionCompanies = 20;
        private const int ViewProductionHouses = 21;
        private const int ManageProductionHouses = 22;
        private const int ViewWorkers = 23;
        private const int ManageWorkers = 24;
        private const int ViewProjects = 25;
        private const int CreateProjects = 26;
        private const int EditProjects = 27;
        private const int DeleteProjects = 28;
        private const int ReviewProjects = 29;
        private const int ManageProjects = 30;

        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // ManageServices added per the associations.routes.ts permission-review correction:
            // ASEZA has broad association oversight but no association-CRUD permission, so it was
            // unreachable for the Association's own service-requests inbox (which now gates on
            // ManageServices alone, not a Manage Association(s) CRUD proxy) until granted directly.
            var asezaPermissions = new[]
            {
                ViewUsers, ViewAssociations, ManageAssociations, ViewAssociationUsers,
                ViewProductionCompanies, ViewProductionHouses, ManageProductionHouses,
                ViewWorkers, ViewProjects, ManageServices
            };

            var productionHousePermissions = new[]
            {
                ViewProjects, CreateProjects, EditProjects,
                RequestService, ViewServiceRequests,
                ViewWorkers
            };

            var associationPermissions = new[]
            {
                ViewAssociations, EditAssociations, ManageWorkers, ManageServices, ViewAssociationUsers
            };

            var allPermissionIds = Enumerable.Range(1, 30);

            var grants = ForRole(BaytAlUrdon, allPermissionIds)
                .Concat(ForRole(Aseza, asezaPermissions))
                .Concat(ForRole(ProductionHouse, productionHousePermissions))
                .Concat(ForRole(Association, associationPermissions));

            builder.HasData(grants);
        }

        private static IEnumerable<RolePermission> ForRole(int roleId, IEnumerable<int> permissionIds)
        {
            return permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                IsActive = true
            });
        }
    }
}

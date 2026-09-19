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
    public class PermissionSeedConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            var seedDate = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

            Permission Seed(int id, string nameEn, string nameAr, string module) => new Permission
            {
                PermissionId = id,
                PermissionNameEn = nameEn,
                PermissionNameAr = nameAr,
                Module = module,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate,
                IsDeleted = false
            };

            builder.HasData(
                // Users
                Seed(1, "View Users", "عرض المستخدمين", "Users"),
                Seed(2, "Create Users", "إنشاء المستخدمين", "Users"),
                Seed(3, "Manage Users", "إدارة المستخدمين", "Users"),

                // Groups
                Seed(4, "View Groups", "عرض المجموعات", "Groups"),
                Seed(5, "Create Groups", "إنشاء المجموعات", "Groups"),
                Seed(6, "Edit Groups", "تعديل المجموعات", "Groups"),
                Seed(7, "Delete Groups", "حذف المجموعات", "Groups"),
                Seed(8, "Manage Groups", "إدارة المجموعات", "Groups"),

                // Associations
                Seed(9, "View Associations", "عرض الجمعيات", "Associations"),
                Seed(10, "Create Associations", "إنشاء الجمعيات", "Associations"),
                Seed(11, "Edit Associations", "تعديل الجمعيات", "Associations"),
                Seed(12, "Delete Associations", "حذف الجمعيات", "Associations"),
                Seed(13, "Manage Associations", "إدارة الجمعيات", "Associations"),
                Seed(14, "View Association Users", "عرض مستخدمي الجمعيات", "Associations"),

                // Service Requests
                Seed(15, "Request Service", "طلب خدمة", "ServiceRequests"),
                Seed(16, "View Service Requests", "عرض طلبات الخدمة", "ServiceRequests"),
                Seed(17, "Manage Service Requests", "إدارة طلبات الخدمة", "ServiceRequests"),
                Seed(18, "Manage Services", "إدارة الخدمات", "ServiceRequests"),

                // Production Companies
                Seed(19, "View Production Companies", "عرض شركات الإنتاج", "ProductionCompanies"),
                Seed(20, "Manage Production Companies", "إدارة شركات الإنتاج", "ProductionCompanies"),

                // Production Houses
                Seed(21, "View Production Houses", "عرض بيوت الإنتاج", "ProductionHouses"),
                Seed(22, "Manage Production Houses", "إدارة بيوت الإنتاج", "ProductionHouses"),

                // Workers
                Seed(23, "View Workers", "عرض العمال", "Workers"),
                Seed(24, "Manage Workers", "إدارة العمال", "Workers"),

                // Projects
                Seed(25, "View Projects", "عرض المشاريع", "Projects"),
                Seed(26, "Create Projects", "إنشاء المشاريع", "Projects"),
                Seed(27, "Edit Projects", "تعديل المشاريع", "Projects"),
                Seed(28, "Delete Projects", "حذف المشاريع", "Projects"),
                Seed(29, "Review Projects", "مراجعة المشاريع", "Projects"),
                Seed(30, "Manage Projects", "إدارة المشاريع", "Projects")
            );
        }
    }
}

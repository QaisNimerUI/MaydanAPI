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
    public class RoleSeedConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            var seedDate = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

            builder.HasData(
                new Role
                {
                    RoleId = 1,
                    RoleNameEn = "Bayt-AlUrdon",
                    RoleNameAr = "بيت الأردن",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                },
                new Role
                {
                    RoleId = 2,
                    RoleNameEn = "ASEZA",
                    RoleNameAr = "سلطة منطقة العقبة الاقتصادية الخاصة",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                },
                new Role
                {
                    RoleId = 3,
                    RoleNameEn = "ProductionHouse",
                    RoleNameAr = "شركة الإنتاج",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                },
                new Role
                {
                    RoleId = 4,
                    RoleNameEn = "Association",
                    RoleNameAr = "الجمعية",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                }
            );
        }
    }
}

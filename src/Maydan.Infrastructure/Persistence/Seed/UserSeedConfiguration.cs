using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;
using Maydan.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maydan.Infrastructure.Persistence.Seed
{
    public class UserSeedConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            var seedDate = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);

            builder.HasData(
                new User
                {
                    UserId = 1,

                    FirstNameEn = "Ghaith",
                    LastNameEn = "Al-Kurdi",

                    FirstNameAr = "غيث",
                    LastNameAr = "الكردي",

                    PhoneNumber = "+962770000023",
                    Email = "ghaith@baytalurdon.org",

                    PasswordHash = "PBKDF2-SHA256.100000.Yil011Rar6X/CCTuFMwT7w==.ClYpfJnPTPSooq0QEiIiMwl0qlPe5xp3LPkjYjNcX10=",

                    RoleId = 1,

                    EntityType = EntityType.BaytAlUrdon,

                    EntityId = 1,

                    MustResetPassword = false,
                    IsActive = true,

                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                }
            );
        }
    }
}

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

                    // Test credentials reset (2026-09-23): the original PBKDF2 hash had no known
                    // plaintext anywhere — Yousef needed a real, working login to test with. New known
                    // password: Test@12345. Hashed with the real IPasswordHasher (PasswordHasher.cs),
                    // not hand-written — see PR/commit description for how it was generated.
                    PasswordHash = "PBKDF2-SHA256.100000.kr2Do1moATCbhMGahUAANg==.2I1SQILgcLQCpWA+3XSAEXhoilqTklMJh0t7BV2LKxo=",

                    RoleId = 1,

                    EntityType = EntityType.BaytAlUrdon,

                    EntityId = 1,

                    MustResetPassword = false,
                    IsActive = true,

                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                },
                new User
                {
                    // Deliberately not 2: the real Dev DB already has UserId 2-14 occupied by leftover
                    // manual/E2E test accounts from earlier work on this project (never real EF seed
                    // rows — HasData only ever defined UserId 1 before this). Picking 1000 keeps this
                    // seeded row out of that ad-hoc range so `dotnet ef database update` doesn't collide
                    // with whatever test data happens to exist in a given environment.
                    UserId = 1000,

                    FirstNameEn = "Aseza",
                    LastNameEn = "Admin",

                    FirstNameAr = "أسيزا",
                    LastNameAr = "أدمن",

                    PhoneNumber = "+962770000024",
                    Email = "aseza-admin@aseza.jo",

                    // Test credentials (2026-09-23): first seeded ASEZA account — none existed before.
                    // Same real IPasswordHasher, password: Aseza@12345.
                    PasswordHash = "PBKDF2-SHA256.100000.NMQd4g85bDIwAs//rBeXvw==.haZXxr9bl/ZmbtsVxJH5pBI2Tz5x42PyO004Wsuv3bo=",

                    RoleId = 2,

                    EntityType = EntityType.Aseza,

                    // Same convention as Ghaith's EntityId = 1 above: EntityType.Aseza is an
                    // administrative entity type with no real backing table (see User.cs's own
                    // comment — EntityId is only a real FK for Association/ProductionCompany), and
                    // there is exactly one ASEZA, so EntityId = 1 by the same placeholder convention.
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

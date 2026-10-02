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
                },
                new User
                {
                    // MAYD-135 (2026-09-24): real Association test account — needed for a real E2E
                    // login while verifying the sidebar-visibility fix, and none of the existing
                    // Association-role rows in Dev (UserId 11-14, ad hoc test debris) have a known
                    // password. Same UserId-1000-range convention as the ASEZA admin above.
                    UserId = 1001,

                    FirstNameEn = "Association",
                    LastNameEn = "Tester",

                    FirstNameAr = "جمعية",
                    LastNameAr = "اختبار",

                    PhoneNumber = "+962770000025",
                    Email = "association-tester@example.org",

                    // Test credentials (2026-09-24). Same real IPasswordHasher, password: Assoc@12345.
                    PasswordHash = "PBKDF2-SHA256.100000.Neiqo3DL9KA6VnPDZZgaWQ==.euR5zQ/Skjf64sJn7SnvR/IDgkd5c/CgJLLfx9HjaXU=",

                    RoleId = 4,

                    EntityType = EntityType.Association,

                    // Unlike BaytAlUrdon/Aseza above, EntityType.Association IS meant to be a real FK
                    // to Association.Id (User.cs's own comment) — but zero real Association rows
                    // exist in this Dev DB (no AssociationsController exists on the backend yet, a
                    // separate, larger, already-flagged gap — see the association-admin-fix task's
                    // report). EntityId = 1 here is a placeholder, not a real reference, same as it
                    // would be for any Association user created today until that gap closes.
                    EntityId = 1,

                    MustResetPassword = false,
                    IsActive = true,

                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    IsDeleted = false
                },
                new User
                {
                    // MAYD-36 (2026-09-24): the entity-scoping verification pass's own explicit gap —
                    // no Production House test account existed anywhere in this project, so this real
                    // role (RoleId 3 / EntityType.ProductionCompany) had never actually been
                    // live-tested across Users/Groups/Permissions, only reasoned about from the other
                    // three roles' behavior. Not UserId 1002 — that id is already occupied by the
                    // app-created `wizard-test@example.org` orphan (not a HasData row), so this
                    // continues the UserId-1000-range convention at the next free id.
                    UserId = 1003,

                    FirstNameEn = "Production",
                    LastNameEn = "Tester",

                    FirstNameAr = "شركة",
                    LastNameAr = "اختبار",

                    PhoneNumber = "+962770000026",
                    Email = "production-tester@example.org",

                    // Test credentials (2026-09-24). Same real IPasswordHasher, password: ProdHouse@12345.
                    PasswordHash = "PBKDF2-SHA256.100000.7DqYchDowXom9IfwCgW+4g==.d41vmNOYVi3nthYMOioczS+W871M/cTKq6Ps58ISAcg=",

                    RoleId = 3,

                    EntityType = EntityType.ProductionCompany,

                    // Same placeholder convention as Association's EntityId = 1 above: EntityType.
                    // ProductionCompany IS meant to be a real FK to ProductionCompany.Id (User.cs's
                    // own comment), but the real ProductionCompanies table has zero rows in this Dev
                    // DB (confirmed via a direct query before writing this seed) — no seeded company
                    // exists to reference yet, same gap noted for Association.
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

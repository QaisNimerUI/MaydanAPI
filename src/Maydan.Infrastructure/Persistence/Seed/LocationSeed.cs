using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Persistence.Seed;

// Associations/Country/City backend gap, Phase 1 (2026-09-25) — the Countries/Cities tables
// existed with zero rows (see LocationService's own header comment for the full story). This app
// is Jordan/Aqaba-specific today (ASEZA coordination, every existing test entity is Jordanian, and
// ProductionHouseSignupComponent/AssociationFormComponent both auto-select "Jordan"/"Amman" by
// name match) — there is no product signal yet for any other country, so this seeds exactly one
// real Country (Jordan) and its 12 real governorates modeled as City rows (Jordan's governorate
// capital shares its governorate's name in every case, and "City" is this schema's only
// administrative-division concept below Country — there is no separate Governorate entity to seed
// instead). Same HasData-in-OnModelCreating convention as ProjectTypeSeed.Seed, migrated the same
// way (EF diffs the HasData against the existing empty tables and emits InsertData).
public static class LocationSeed
{
    public const int JordanCountryId = 1;

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>().HasData(
            new Country
            {
                Id = JordanCountryId,
                EnglishName = "Jordan",
                ArabicName = "الأردن",
                IsActive = true,
                IsDeleted = false
            });

        modelBuilder.Entity<City>().HasData(
            new City { Id = 1, CountryId = JordanCountryId, EnglishName = "Amman", ArabicName = "عمان", IsActive = true, IsDeleted = false },
            new City { Id = 2, CountryId = JordanCountryId, EnglishName = "Irbid", ArabicName = "إربد", IsActive = true, IsDeleted = false },
            new City { Id = 3, CountryId = JordanCountryId, EnglishName = "Zarqa", ArabicName = "الزرقاء", IsActive = true, IsDeleted = false },
            new City { Id = 4, CountryId = JordanCountryId, EnglishName = "Balqa", ArabicName = "البلقاء", IsActive = true, IsDeleted = false },
            new City { Id = 5, CountryId = JordanCountryId, EnglishName = "Madaba", ArabicName = "مأدبا", IsActive = true, IsDeleted = false },
            new City { Id = 6, CountryId = JordanCountryId, EnglishName = "Karak", ArabicName = "الكرك", IsActive = true, IsDeleted = false },
            new City { Id = 7, CountryId = JordanCountryId, EnglishName = "Tafilah", ArabicName = "الطفيلة", IsActive = true, IsDeleted = false },
            new City { Id = 8, CountryId = JordanCountryId, EnglishName = "Ma'an", ArabicName = "معان", IsActive = true, IsDeleted = false },
            new City { Id = 9, CountryId = JordanCountryId, EnglishName = "Aqaba", ArabicName = "العقبة", IsActive = true, IsDeleted = false },
            new City { Id = 10, CountryId = JordanCountryId, EnglishName = "Jerash", ArabicName = "جرش", IsActive = true, IsDeleted = false },
            new City { Id = 11, CountryId = JordanCountryId, EnglishName = "Ajloun", ArabicName = "عجلون", IsActive = true, IsDeleted = false },
            new City { Id = 12, CountryId = JordanCountryId, EnglishName = "Mafraq", ArabicName = "المفرق", IsActive = true, IsDeleted = false }
        );
    }
}

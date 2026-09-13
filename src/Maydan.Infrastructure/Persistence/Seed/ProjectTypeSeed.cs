using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Persistence.Seed;

public static class ProjectTypeSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectType>().HasData(
            new ProjectType
            {
                Id = 1,
                NameAr = "عرض واقعي",
                NameEn = "Reality Show",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 2,
                NameAr = "فيديو موسيقي",
                NameEn = "Music Video",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 3,
                NameAr = "إعلانات متلفزة",
                NameEn = "Commercials",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 4,
                NameAr = "فيلم قصير",
                NameEn = "Short Film",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 5,
                NameAr = "فيلم طويل",
                NameEn = "Feature Film",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 6,
                NameAr = "صور متحركة",
                NameEn = "Animation",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 7,
                NameAr = "تصوير فوتوغرافي",
                NameEn = "Photography",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 8,
                NameAr = "برامج",
                NameEn = "TV Program",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 9,
                NameAr = "مسلسل",
                NameEn = "Series",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 10,
                NameAr = "ألعاب تفاعلية",
                NameEn = "Interactive/Game",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 11,
                NameAr = "وثائقي طويل",
                NameEn = "Feature Documentary",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 12,
                NameAr = "وثائقي قصير",
                NameEn = "Short Documentary",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 13,
                NameAr = "مسلسل وثائقي",
                NameEn = "Documentary Series",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 14,
                NameAr = "وثائقي صناعي/شركات",
                NameEn = "Corporate/Industrial Documentary",
                IsActive = true,
                IsDeleted = false
            },
            new ProjectType
            {
                Id = 15,
                NameAr = "إنتاج طلابي",
                NameEn = "Student Film",
                IsActive = true,
                IsDeleted = false
            }
        );
    }
}
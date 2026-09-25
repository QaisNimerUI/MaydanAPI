namespace Maydan.Application.DTOs.Locations;

// Field names below are deliberately NOT the usual EnglishName/ArabicName pair this codebase uses
// elsewhere (Project, ProjectType, Group, ...) — they match workforcment's own
// src/app/features/locations/models/location.models.ts exactly (CountryEnglishName ->
// countryEnglishName once System.Text.Json's default camelCase policy applies), which is the real,
// already-written frontend contract this DTO has to satisfy, not a convention this pass gets to
// pick.
public class CountryDto
{
    public int Id { get; set; }
    public string CountryEnglishName { get; set; } = string.Empty;
    public string CountryArabicName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}

public class CreateCountryDto
{
    public string CountryEnglishName { get; set; } = string.Empty;
    public string CountryArabicName { get; set; } = string.Empty;
}

public class UpdateCountryDto
{
    public int Id { get; set; }
    public string CountryEnglishName { get; set; } = string.Empty;
    public string CountryArabicName { get; set; } = string.Empty;
}

public class CityDto
{
    public int Id { get; set; }
    public string CityEnglishName { get; set; } = string.Empty;
    public string CityArabicName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateCityDto
{
    public string CityEnglishName { get; set; } = string.Empty;
    public string CityArabicName { get; set; } = string.Empty;
    public int CountryId { get; set; }
}

public class UpdateCityDto
{
    public string CityEnglishName { get; set; } = string.Empty;
    public string CityArabicName { get; set; } = string.Empty;
    public int CountryId { get; set; }
}

using Maydan.Domain.Enums;

namespace Maydan.Application.DTOs.Services;

public record ServiceDto(
    int Id,
    string NameEn,
    string NameAr,
    CalculationType CalculationType,
    decimal Price,
    bool IsActive
);

public class CreateServiceDto
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public CalculationType CalculationType { get; set; }
    public decimal Price { get; set; }
}

public class UpdateServiceDto
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public CalculationType CalculationType { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class CityLocation : SharedEntities
{
    public string EnglishName { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;

    public int CityId { get; set; }
    public City City { get; set; } = null!;
}

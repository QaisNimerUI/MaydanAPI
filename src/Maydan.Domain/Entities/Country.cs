using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class Country : SharedEntities
{
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;

    public ICollection<City> Cities { get; set; } = new List<City>();
}

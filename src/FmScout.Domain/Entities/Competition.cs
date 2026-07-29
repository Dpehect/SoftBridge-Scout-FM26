using FmScout.Domain.Common;
namespace FmScout.Domain.Entities;
public sealed class Competition : Entity
{
    private Competition() { }
    public Competition(string name, string slug, Guid countryId) { Name = name; Slug = slug; CountryId = countryId; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = null!;
    public ICollection<Club> Clubs { get; private set; } = [];
}

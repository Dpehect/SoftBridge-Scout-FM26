using FmScout.Domain.Common;
namespace FmScout.Domain.Entities;
public sealed class Country : Entity
{
    private Country() { }
    public Country(string name, string code) { Name = name; Code = code.ToUpperInvariant(); }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public ICollection<Club> Clubs { get; private set; } = [];
}

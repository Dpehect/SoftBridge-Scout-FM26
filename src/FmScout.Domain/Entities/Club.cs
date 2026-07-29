using FmScout.Domain.Common;
namespace FmScout.Domain.Entities;
public sealed class Club : Entity
{
    private Club() { }
    public Club(string name, string slug, string shortName, Guid countryId, Guid? competitionId = null)
    { Name = name; Slug = slug; ShortName = shortName; CountryId = countryId; CompetitionId = competitionId; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = null!;
    public Guid? CompetitionId { get; private set; }
    public Competition? Competition { get; private set; }
    public int Reputation { get; private set; }
    public ICollection<Player> Players { get; private set; } = [];
}

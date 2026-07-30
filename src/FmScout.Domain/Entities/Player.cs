namespace FmScout.Domain.Entities;
public sealed class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string Nation { get; set; } = string.Empty;
    public int CurrentAbility { get; set; }
    public int PotentialAbility { get; set; }
    public decimal MarketValue { get; set; }
    public bool IsWonderkid { get; set; }
    public bool IsHiddenGem { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

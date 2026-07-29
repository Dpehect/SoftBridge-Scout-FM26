using FmScout.Domain.Common;
namespace FmScout.Domain.Entities;
public sealed class TacticalRole : Entity
{
    private TacticalRole() { }
    public TacticalRole(string name, string slug, string positionCode) { Name = name; Slug = slug; PositionCode = positionCode; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string PositionCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
}

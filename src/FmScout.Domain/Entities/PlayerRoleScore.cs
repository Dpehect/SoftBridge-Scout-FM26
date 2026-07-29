using FmScout.Domain.Common;
namespace FmScout.Domain.Entities;
public sealed class PlayerRoleScore : Entity
{
    private PlayerRoleScore() { }
    public PlayerRoleScore(Guid playerId, Guid tacticalRoleId, decimal score) { PlayerId = playerId; TacticalRoleId = tacticalRoleId; Score = score; }
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = null!;
    public Guid TacticalRoleId { get; private set; }
    public TacticalRole TacticalRole { get; private set; } = null!;
    public decimal Score { get; private set; }
}

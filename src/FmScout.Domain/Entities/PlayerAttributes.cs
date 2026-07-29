using FmScout.Domain.Common;
namespace FmScout.Domain.Entities;
public sealed class PlayerAttributes : Entity
{
    private PlayerAttributes() { }
    public PlayerAttributes(Guid playerId) => PlayerId = playerId;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = null!;
    public int Acceleration { get; set; } public int Pace { get; set; } public int Stamina { get; set; } public int Strength { get; set; }
    public int Passing { get; set; } public int Technique { get; set; } public int FirstTouch { get; set; } public int Dribbling { get; set; }
    public int Finishing { get; set; } public int Tackling { get; set; } public int Marking { get; set; } public int Crossing { get; set; }
    public int Decisions { get; set; } public int Vision { get; set; } public int WorkRate { get; set; } public int Teamwork { get; set; }
    public int Anticipation { get; set; } public int Composure { get; set; } public int Determination { get; set; } public int Positioning { get; set; }
}

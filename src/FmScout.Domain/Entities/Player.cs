using FmScout.Domain.Common;
using FmScout.Domain.Enums;
namespace FmScout.Domain.Entities;
public sealed class Player : Entity
{
    private Player() { }
    public Player(string firstName, string lastName, string slug, DateOnly dateOfBirth, Guid countryId)
    { FirstName = firstName; LastName = lastName; Slug = slug; DateOfBirth = dateOfBirth; CountryId = countryId; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Slug { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - (DateOnly.FromDateTime(DateTime.UtcNow) < DateOfBirth.AddYears(DateTime.UtcNow.Year - DateOfBirth.Year) ? 1 : 0);
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = null!;
    public Guid? ClubId { get; private set; }
    public Club? Club { get; private set; }
    public string PrimaryPosition { get; private set; } = "CM";
    public string SecondaryPositions { get; private set; } = string.Empty;
    public PreferredFoot PreferredFoot { get; private set; }
    public PlayerStatus Status { get; private set; } = PlayerStatus.Active;
    public int CurrentAbility { get; private set; }
    public int PotentialAbility { get; private set; }
    public decimal MarketValue { get; private set; }
    public decimal WeeklyWage { get; private set; }
    public DateOnly? ContractExpiresAt { get; private set; }
    public int HeightCm { get; private set; }
    public string Personality { get; private set; } = string.Empty;
    public string MediaDescription { get; private set; } = string.Empty;
    public bool IsWonderkid { get; private set; }
    public bool IsFeatured { get; private set; }
    public PlayerAttributes Attributes { get; private set; } = null!;
    public ICollection<PlayerRoleScore> RoleScores { get; private set; } = [];
    public void UpdateIdentity(string firstName, string lastName, string slug, DateOnly dateOfBirth, Guid countryId) { FirstName = firstName; LastName = lastName; Slug = slug; DateOfBirth = dateOfBirth; CountryId = countryId; Touch(); }
    public void AssignClub(Guid? clubId) { ClubId = clubId; Touch(); }
    public void SetProfile(string primaryPosition, string secondaryPositions, PreferredFoot foot, int heightCm, string personality, string mediaDescription)
    { PrimaryPosition = primaryPosition; SecondaryPositions = secondaryPositions; PreferredFoot = foot; HeightCm = heightCm; Personality = personality; MediaDescription = mediaDescription; Touch(); }
    public void SetValuation(int currentAbility, int potentialAbility, decimal marketValue, decimal weeklyWage, DateOnly? contractExpiresAt)
    { CurrentAbility = currentAbility; PotentialAbility = potentialAbility; MarketValue = marketValue; WeeklyWage = weeklyWage; ContractExpiresAt = contractExpiresAt; IsWonderkid = Age <= 21 && potentialAbility >= 155; Touch(); }
    public void SetFeatured(bool value) { IsFeatured = value; Touch(); }
}

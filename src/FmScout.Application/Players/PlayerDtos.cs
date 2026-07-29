using FmScout.Domain.Enums;
namespace FmScout.Application.Players;
public sealed record PlayerListItemDto(Guid Id, string Slug, string FullName, int Age, string Country, string CountryCode, string? Club, string Position, PreferredFoot PreferredFoot, int CurrentAbility, int PotentialAbility, decimal MarketValue, decimal WeeklyWage, bool IsWonderkid, bool IsFeatured);
public sealed record AttributeGroupDto(string Name, IReadOnlyDictionary<string, int> Values);
public sealed record RoleScoreDto(string Role, string Slug, decimal Score);
public sealed record PlayerDetailDto(Guid Id, string Slug, string FullName, int Age, string Country, string CountryCode, string? Club, string Position, string SecondaryPositions, PreferredFoot PreferredFoot, int HeightCm, string Personality, string MediaDescription, int CurrentAbility, int PotentialAbility, decimal MarketValue, decimal WeeklyWage, DateOnly? ContractExpiresAt, bool IsWonderkid, bool IsFeatured, IReadOnlyList<AttributeGroupDto> AttributeGroups, IReadOnlyList<RoleScoreDto> RoleScores);

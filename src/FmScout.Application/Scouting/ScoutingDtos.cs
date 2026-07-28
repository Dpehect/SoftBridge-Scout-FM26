namespace FmScout.Application.Scouting;

public sealed record ScoutRequest(
    string? Position = null,
    int? MaxAge = null,
    decimal? MaxMarketValue = null,
    int? MinPotential = null,
    string? Role = null,
    int Limit = 12);

public sealed record ScoutRecommendationDto(
    Guid Id,
    string Slug,
    string FullName,
    int Age,
    string Position,
    string Country,
    string? Club,
    decimal MarketValue,
    int CurrentAbility,
    int PotentialAbility,
    decimal ScoutScore,
    decimal ValueScore,
    decimal DevelopmentScore,
    decimal RoleScore,
    IReadOnlyList<string> Reasons);

public sealed record ComparisonPlayerDto(
    Guid Id,
    string Slug,
    string FullName,
    int Age,
    string Position,
    decimal MarketValue,
    decimal WeeklyWage,
    int CurrentAbility,
    int PotentialAbility,
    decimal BestRoleScore,
    IReadOnlyDictionary<string, decimal> CategoryScores);

public sealed record PlayerComparisonDto(
    IReadOnlyList<ComparisonPlayerDto> Players,
    string ImmediateImpactWinner,
    string LongTermWinner,
    string ValueWinner);

public sealed record PlatformStatsDto(int Players, int Clubs, int Countries, int Roles, int Wonderkids);

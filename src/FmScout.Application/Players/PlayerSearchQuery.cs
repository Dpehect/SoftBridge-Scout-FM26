namespace FmScout.Application.Players;
public sealed record PlayerSearchQuery(
    string? Search = null,
    string? Position = null,
    string? Country = null,
    string? Club = null,
    int? MinAge = null,
    int? MaxAge = null,
    int? MinPotential = null,
    decimal? MaxMarketValue = null,
    bool? Wonderkid = null,
    bool? Featured = null,
    string Sort = "potential-desc",
    int Page = 1,
    int PageSize = 20);

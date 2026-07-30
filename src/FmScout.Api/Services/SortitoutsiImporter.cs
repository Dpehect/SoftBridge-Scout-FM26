using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FmScout.Api.Services;

public sealed partial class SortitoutsiImporter(
    HttpClient http,
    FmScoutDbContext db,
    ILogger<SortitoutsiImporter> logger)
{
    private const string BaseUrl = "https://sortitoutsi.net";
    private static readonly TimeSpan RequestDelay = TimeSpan.FromMilliseconds(1800);

    public static readonly IReadOnlyList<LeagueTarget> Leagues =
    [
        new("England", "Premier League"),
        new("Spain", "LaLiga"),
        new("Italy", "Serie A"),
        new("Germany", "Bundesliga"),
        new("France", "Ligue 1"),
        new("Portugal", "Primeira Liga"),
        new("Netherlands", "Eredivisie"),
        new("Belgium", "Jupiler Pro League"),
        new("Türkiye", "Trendyol Süper Lig"),
        new("Brazil", "Brasileirão Série A")
    ];

    public async Task<ImportResult> RunBatchAsync(int leagueIndex, int clubOffset, int clubLimit, CancellationToken ct)
    {
        if (leagueIndex < 0 || leagueIndex >= Leagues.Count)
            throw new ArgumentOutOfRangeException(nameof(leagueIndex));

        clubOffset = Math.Max(0, clubOffset);
        clubLimit = Math.Clamp(clubLimit, 1, 5);
        var target = Leagues[leagueIndex];

        var indexHtml = await GetAsync("/football-manager-2026/database", ct);
        var nationUrl = FindAnchorUrl(indexHtml, "/football-manager-2026/nation/", target.Nation)
            ?? throw new InvalidOperationException($"Nation page not found: {target.Nation}");

        await DelayAsync(ct);
        var nationHtml = await GetAsync(nationUrl, ct);
        var leagueUrl = FindAnchorUrl(nationHtml, "/football-manager-2026/competition/", target.League)
            ?? FindCompetitionByAliases(nationHtml, target)
            ?? throw new InvalidOperationException($"Competition page not found: {target.League}");

        await DelayAsync(ct);
        var leagueHtml = await GetAsync(leagueUrl, ct);
        var clubs = FindLinks(leagueHtml, "/football-manager-2026/team/")
            .DistinctBy(x => x.Url, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var selected = clubs.Skip(clubOffset).Take(clubLimit).ToArray();
        var inserted = 0;
        var updated = 0;
        var skipped = 0;

        foreach (var club in selected)
        {
            await DelayAsync(ct);
            var clubHtml = await GetAsync(club.Url, ct);
            var players = ParsePlayers(clubHtml, club.Text).Take(120).ToArray();

            foreach (var row in players)
            {
                var existing = await db.Players.FirstOrDefaultAsync(x => x.Slug == row.Slug, ct);
                if (existing is null)
                {
                    db.Players.Add(ToEntity(row));
                    inserted++;
                }
                else
                {
                    Apply(existing, row);
                    updated++;
                }
            }

            skipped += Math.Max(0, players.Length - 120);
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }

        logger.LogInformation(
            "Sortitoutsi batch completed. League={League}, Offset={Offset}, Clubs={ClubCount}, Inserted={Inserted}, Updated={Updated}",
            target.League, clubOffset, selected.Length, inserted, updated);

        return new ImportResult(
            target.Nation,
            target.League,
            clubOffset,
            selected.Length,
            clubs.Length,
            inserted,
            updated,
            skipped,
            clubOffset + selected.Length,
            clubOffset + selected.Length >= clubs.Length);
    }

    private async Task<string> GetAsync(string url, CancellationToken ct)
    {
        var absolute = url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : BaseUrl + url;
        using var request = new HttpRequestMessage(HttpMethod.Get, absolute);
        request.Headers.UserAgent.ParseAdd("SoftBridgeScoutFM26/1.0 (+permission-granted; contact=site-owner)");
        request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml");
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    private static Task DelayAsync(CancellationToken ct) => Task.Delay(RequestDelay, ct);

    private static string? FindCompetitionByAliases(string html, LeagueTarget target)
    {
        var aliases = target.Nation switch
        {
            "Spain" => new[] { "La Liga", "Primera División" },
            "Italy" => new[] { "Serie A" },
            "Germany" => new[] { "Bundesliga" },
            "France" => new[] { "Ligue 1" },
            "Portugal" => new[] { "Liga Portugal", "Primeira Liga" },
            "Netherlands" => new[] { "Eredivisie" },
            "Belgium" => new[] { "Pro League", "Jupiler Pro League" },
            "Türkiye" => new[] { "Süper Lig", "Super Lig" },
            "Brazil" => new[] { "Série A", "Serie A", "Brasileirão" },
            _ => Array.Empty<string>()
        };

        return aliases.Select(x => FindAnchorUrl(html, "/football-manager-2026/competition/", x))
            .FirstOrDefault(x => x is not null);
    }

    private static string? FindAnchorUrl(string html, string requiredPath, string expectedText) =>
        FindLinks(html, requiredPath)
            .FirstOrDefault(x => Normalize(x.Text).Contains(Normalize(expectedText), StringComparison.OrdinalIgnoreCase))?.Url;

    private static IEnumerable<LinkInfo> FindLinks(string html, string requiredPath)
    {
        foreach (Match match in AnchorRegex().Matches(html))
        {
            var url = WebUtility.HtmlDecode(match.Groups[1].Value);
            if (!url.Contains(requiredPath, StringComparison.OrdinalIgnoreCase)) continue;
            var text = StripHtml(match.Groups[2].Value);
            if (string.IsNullOrWhiteSpace(text)) continue;
            yield return new LinkInfo(url, text);
        }
    }

    private static IEnumerable<ScrapedPlayer> ParsePlayers(string html, string fallbackClub)
    {
        foreach (Match rowMatch in RowRegex().Matches(html))
        {
            var rowHtml = rowMatch.Groups[1].Value;
            var playerLink = FindLinks(rowHtml, "/football-manager-2026/player/").FirstOrDefault();
            if (playerLink is null) continue;

            var cells = CellRegex().Matches(rowHtml).Select(x => StripHtml(x.Groups[1].Value)).ToArray();
            if (cells.Length < 7) continue;

            var playerId = ExtractNumericId(playerLink.Url, "/player/");
            var slug = BuildSlug(playerLink.Text, playerId);
            var nation = FindLinks(rowHtml, "/football-manager-2026/nation/").FirstOrDefault()?.Text ?? "Unknown";
            var age = ParseInt(cells.ElementAtOrDefault(2));
            var position = cells.ElementAtOrDefault(3) ?? string.Empty;
            var marketValue = ParseMoney(cells.ElementAtOrDefault(5));
            var rating = ParsePercent(cells.ElementAtOrDefault(8));
            var potential = ParsePercent(cells.ElementAtOrDefault(9));

            yield return new ScrapedPlayer(
                playerLink.Text,
                slug,
                age,
                position,
                fallbackClub,
                nation,
                rating,
                potential,
                marketValue,
                age is > 0 and <= 21 && potential >= 80,
                marketValue <= 5_000_000m && potential >= 75);
        }
    }

    private static Player ToEntity(ScrapedPlayer row)
    {
        var player = new Player();
        Apply(player, row);
        return player;
    }

    private static void Apply(Player player, ScrapedPlayer row)
    {
        player.Name = row.Name;
        player.Slug = row.Slug;
        player.Age = row.Age;
        player.Position = row.Position;
        player.Club = row.Club;
        player.Nation = row.Nation;
        player.CurrentAbility = row.CurrentAbility;
        player.PotentialAbility = row.PotentialAbility;
        player.MarketValue = row.MarketValue;
        player.IsWonderkid = row.IsWonderkid;
        player.IsHiddenGem = row.IsHiddenGem;
    }

    private static int ParseInt(string? value) => int.TryParse(value, out var result) ? result : 0;

    private static int ParsePercent(string? value)
    {
        var digits = DigitsRegex().Match(value ?? string.Empty).Value;
        return int.TryParse(digits, out var result) ? Math.Clamp(result, 0, 200) : 0;
    }

    private static decimal ParseMoney(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var normalized = value.Replace("£", "", StringComparison.Ordinal)
            .Replace("€", "", StringComparison.Ordinal)
            .Replace("$", "", StringComparison.Ordinal)
            .Replace(",", "", StringComparison.Ordinal)
            .Trim();

        var multiplier = normalized.EndsWith('m', StringComparison.OrdinalIgnoreCase) ? 1_000_000m
            : normalized.EndsWith('k', StringComparison.OrdinalIgnoreCase) ? 1_000m
            : 1m;
        normalized = normalized.TrimEnd('m', 'M', 'k', 'K');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount * multiplier
            : 0;
    }

    private static string ExtractNumericId(string url, string marker)
    {
        var index = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return Math.Abs(url.GetHashCode(StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture);
        var remainder = url[(index + marker.Length)..];
        return remainder.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "0";
    }

    private static string BuildSlug(string name, string sourceId)
    {
        var slug = NonSlugRegex().Replace(Normalize(name).ToLowerInvariant(), "-").Trim('-');
        return $"{slug}-{sourceId}";
    }

    private static string Normalize(string value) => WebUtility.HtmlDecode(value).Trim();

    private static string StripHtml(string value) =>
        Normalize(TagRegex().Replace(value, " ")).Replace("  ", " ", StringComparison.Ordinal);

    [GeneratedRegex("<a[^>]+href=[\"']([^\"']+)[\"'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex("<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex RowRegex();

    [GeneratedRegex("<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CellRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagRegex();

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.IgnoreCase)]
    private static partial Regex NonSlugRegex();

    [GeneratedRegex("\\d+")]
    private static partial Regex DigitsRegex();
}

public sealed record LeagueTarget(string Nation, string League);
public sealed record LinkInfo(string Url, string Text);
public sealed record ScrapedPlayer(string Name, string Slug, int Age, string Position, string Club, string Nation, int CurrentAbility, int PotentialAbility, decimal MarketValue, bool IsWonderkid, bool IsHiddenGem);
public sealed record ImportResult(string Nation, string League, int ClubOffset, int ClubsProcessed, int TotalClubs, int Inserted, int Updated, int Skipped, int NextClubOffset, bool LeagueCompleted);
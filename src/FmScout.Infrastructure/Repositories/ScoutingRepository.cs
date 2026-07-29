using FmScout.Application.Abstractions;
using FmScout.Application.Scouting;
using FmScout.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FmScout.Infrastructure.Repositories;

public sealed class ScoutingRepository(FmScoutDbContext db) : IScoutingRepository
{
    public async Task<IReadOnlyList<ScoutRecommendationDto>> RecommendAsync(ScoutRequest request, CancellationToken ct)
    {
        var query = db.Players.AsNoTracking()
            .Include(x => x.Country).Include(x => x.Club).Include(x => x.RoleScores).ThenInclude(x => x.TacticalRole)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Position)) query = query.Where(x => x.PrimaryPosition == request.Position);
        if (request.MaxAge is not null)
        {
            var minBirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-request.MaxAge.Value - 1).AddDays(1));
            query = query.Where(x => x.DateOfBirth >= minBirthDate);
        }
        if (request.MaxMarketValue is not null) query = query.Where(x => x.MarketValue <= request.MaxMarketValue);
        if (request.MinPotential is not null) query = query.Where(x => x.PotentialAbility >= request.MinPotential);

        var players = await query.Take(250).ToListAsync(ct);
        var maxBudget = request.MaxMarketValue ?? Math.Max(1m, players.Count == 0 ? 1m : players.Max(x => x.MarketValue));

        return players.Select(p =>
        {
            var age = p.Age;
            var development = Math.Clamp((p.PotentialAbility - p.CurrentAbility) * 3.2m + (24 - Math.Min(age, 24)) * 2m, 0m, 100m);
            var value = Math.Clamp(100m - (p.MarketValue / Math.Max(maxBudget, 1m) * 70m), 0m, 100m);
            var role = string.IsNullOrWhiteSpace(request.Role)
                ? p.RoleScores.Select(x => x.Score).DefaultIfEmpty(65m).Max()
                : p.RoleScores.Where(x => x.TacticalRole.Slug == request.Role).Select(x => x.Score).DefaultIfEmpty(50m).Max();
            var quality = p.CurrentAbility / 2m;
            var potential = p.PotentialAbility / 2m;
            var total = Math.Round(quality * .25m + potential * .25m + development * .20m + value * .15m + role * .15m, 1);
            var reasons = new List<string>();
            if (p.PotentialAbility >= 180) reasons.Add("Elit potansiyel");
            if (development >= 70) reasons.Add("Yüksek gelişim marjı");
            if (value >= 70) reasons.Add("Bütçe dostu profil");
            if (role >= 82) reasons.Add("Güçlü rol uyumu");
            if (reasons.Count == 0) reasons.Add("Dengeli scout profili");
            return new ScoutRecommendationDto(p.Id, p.Slug, p.FullName, age, p.PrimaryPosition, p.Country.Name, p.Club?.Name,
                p.MarketValue, p.CurrentAbility, p.PotentialAbility, total, Math.Round(value, 1), Math.Round(development, 1), Math.Round(role, 1), reasons);
        }).OrderByDescending(x => x.ScoutScore).Take(request.Limit).ToList();
    }

    public async Task<PlayerComparisonDto> CompareAsync(IReadOnlyCollection<string> slugs, CancellationToken ct)
    {
        var players = await db.Players.AsNoTracking().Include(x => x.Attributes).Include(x => x.RoleScores)
            .Where(x => slugs.Contains(x.Slug)).ToListAsync(ct);
        if (players.Count < 2) throw new KeyNotFoundException("Karşılaştırılacak oyuncular bulunamadı.");

        var result = players.Select(p =>
        {
            var a = p.Attributes;
            var technical = new decimal[] { a.Passing, a.Technique, a.FirstTouch, a.Dribbling, a.Finishing, a.Tackling, a.Marking, a.Crossing }.Average() * 5m;
            var mental = new decimal[] { a.Decisions, a.Vision, a.WorkRate, a.Teamwork, a.Anticipation, a.Composure, a.Determination, a.Positioning }.Average() * 5m;
            var physical = new decimal[] { a.Acceleration, a.Pace, a.Stamina, a.Strength }.Average() * 5m;
            return new ComparisonPlayerDto(p.Id, p.Slug, p.FullName, p.Age, p.PrimaryPosition, p.MarketValue, p.WeeklyWage,
                p.CurrentAbility, p.PotentialAbility, p.RoleScores.Select(x => x.Score).DefaultIfEmpty(0).Max(),
                new Dictionary<string, decimal> { ["Teknik"] = Math.Round(technical, 1), ["Mental"] = Math.Round(mental, 1), ["Fiziksel"] = Math.Round(physical, 1) });
        }).ToList();

        return new PlayerComparisonDto(result,
            result.MaxBy(x => x.CurrentAbility)!.FullName,
            result.MaxBy(x => x.PotentialAbility)!.FullName,
            result.MaxBy(x => (x.CurrentAbility + x.PotentialAbility) / Math.Max(x.MarketValue / 1_000_000m, 1m))!.FullName);
    }

    public async Task<PlatformStatsDto> GetStatsAsync(CancellationToken ct) => new(
        await db.Players.CountAsync(ct), await db.Clubs.CountAsync(ct), await db.Countries.CountAsync(ct),
        await db.TacticalRoles.CountAsync(ct), await db.Players.CountAsync(x => x.IsWonderkid, ct));
}

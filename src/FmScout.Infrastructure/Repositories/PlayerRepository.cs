using FmScout.Application.Abstractions;
using FmScout.Application.Common;
using FmScout.Application.Players;
using FmScout.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Infrastructure.Repositories;
public sealed class PlayerRepository(FmScoutDbContext db) : IPlayerRepository
{
    public async Task<PagedResult<PlayerListItemDto>> SearchAsync(PlayerSearchQuery query, CancellationToken ct)
    {
        var players = db.Players.AsNoTracking().Include(x => x.Country).Include(x => x.Club).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search)) { var q = query.Search.Trim().ToLower(); players = players.Where(x => (x.FirstName + " " + x.LastName).ToLower().Contains(q)); }
        if (!string.IsNullOrWhiteSpace(query.Position)) players = players.Where(x => x.PrimaryPosition == query.Position);
        if (!string.IsNullOrWhiteSpace(query.Country)) players = players.Where(x => x.Country.Code == query.Country || x.Country.Name == query.Country);
        if (!string.IsNullOrWhiteSpace(query.Club)) players = players.Where(x => x.Club != null && x.Club.Slug == query.Club);
        if (query.MinAge is not null) { var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-query.MinAge.Value - 1)); players = players.Where(x => x.DateOfBirth <= cutoff); }
        if (query.MaxAge is not null) { var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-query.MaxAge.Value)); players = players.Where(x => x.DateOfBirth >= cutoff); }
        if (query.MinPotential is not null) players = players.Where(x => x.PotentialAbility >= query.MinPotential);
        if (query.MaxMarketValue is not null) players = players.Where(x => x.MarketValue <= query.MaxMarketValue);
        if (query.Wonderkid is not null) players = players.Where(x => x.IsWonderkid == query.Wonderkid);
        if (query.Featured is not null) players = players.Where(x => x.IsFeatured == query.Featured);
        players = query.Sort switch { "value-asc" => players.OrderBy(x => x.MarketValue), "value-desc" => players.OrderByDescending(x => x.MarketValue), "age-asc" => players.OrderByDescending(x => x.DateOfBirth), "ability-desc" => players.OrderByDescending(x => x.CurrentAbility), _ => players.OrderByDescending(x => x.PotentialAbility).ThenByDescending(x => x.CurrentAbility) };
        var total = await players.CountAsync(ct);
        var items = await players.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new PlayerListItemDto(x.Id, x.Slug, x.FirstName + " " + x.LastName, DateTime.UtcNow.Year - x.DateOfBirth.Year, x.Country.Name, x.Country.Code, x.Club != null ? x.Club.Name : null, x.PrimaryPosition, x.PreferredFoot, x.CurrentAbility, x.PotentialAbility, x.MarketValue, x.WeeklyWage, x.IsWonderkid, x.IsFeatured)).ToListAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }
    public async Task<PlayerDetailDto?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var x = await db.Players.AsNoTracking().Include(p => p.Country).Include(p => p.Club).Include(p => p.Attributes).Include(p => p.RoleScores).ThenInclude(r => r.TacticalRole).SingleOrDefaultAsync(p => p.Slug == slug, ct);
        if (x is null) return null;
        var a = x.Attributes;
        var groups = new List<AttributeGroupDto> {
            new("Technical", new Dictionary<string,int>{{"Passing",a.Passing},{"Technique",a.Technique},{"First Touch",a.FirstTouch},{"Dribbling",a.Dribbling},{"Finishing",a.Finishing},{"Tackling",a.Tackling},{"Marking",a.Marking},{"Crossing",a.Crossing}}),
            new("Mental", new Dictionary<string,int>{{"Decisions",a.Decisions},{"Vision",a.Vision},{"Work Rate",a.WorkRate},{"Teamwork",a.Teamwork},{"Anticipation",a.Anticipation},{"Composure",a.Composure},{"Determination",a.Determination},{"Positioning",a.Positioning}}),
            new("Physical", new Dictionary<string,int>{{"Acceleration",a.Acceleration},{"Pace",a.Pace},{"Stamina",a.Stamina},{"Strength",a.Strength}})
        };
        return new(x.Id,x.Slug,x.FullName,x.Age,x.Country.Name,x.Country.Code,x.Club?.Name,x.PrimaryPosition,x.SecondaryPositions,x.PreferredFoot,x.HeightCm,x.Personality,x.MediaDescription,x.CurrentAbility,x.PotentialAbility,x.MarketValue,x.WeeklyWage,x.ContractExpiresAt,x.IsWonderkid,x.IsFeatured,groups,x.RoleScores.OrderByDescending(r=>r.Score).Select(r=>new RoleScoreDto(r.TacticalRole.Name,r.TacticalRole.Slug,r.Score)).ToList());
    }
}

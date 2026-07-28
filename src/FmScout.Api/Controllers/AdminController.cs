using System.Globalization;
using System.Text;
using FmScout.Api.Security;
using FmScout.Domain.Entities;
using FmScout.Domain.Enums;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FmScout.Api.Controllers;

[ApiController, Route("api/admin"), AdminKey]
public sealed class AdminController(FmScoutDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(new
    {
        players = await db.Players.CountAsync(ct),
        clubs = await db.Clubs.CountAsync(ct),
        countries = await db.Countries.CountAsync(ct),
        competitions = await db.Competitions.CountAsync(ct),
        articles = await db.Articles.CountAsync(ct),
        collections = await db.PlayerCollections.CountAsync(ct),
        shortlists = await db.Shortlists.CountAsync(ct),
        users = await db.UserAccounts.CountAsync(ct),
        audit = await db.AuditLogs.OrderByDescending(x => x.CreatedAt).Take(30).ToListAsync(ct)
    });

    [HttpGet("players")]
    public async Task<IActionResult> Players([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Players.AsNoTracking().Include(x => x.Country).Include(x => x.Club).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => (x.FirstName + " " + x.LastName).ToLower().Contains(q.ToLower()) || x.Slug.Contains(q.ToLower()));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.LastName).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Id, x.Slug, x.FirstName, x.LastName, x.PrimaryPosition, x.CurrentAbility, x.PotentialAbility, x.MarketValue, country = x.Country.Name, club = x.Club == null ? null : x.Club.Name })
            .ToListAsync(ct);
        return Ok(new { items, page, pageSize, total });
    }

    [HttpPost("players")]
    public async Task<IActionResult> CreatePlayer([FromBody] PlayerUpsertRequest body, CancellationToken ct)
    {
        var error = await ValidatePlayer(body, null, ct); if (error is not null) return BadRequest(new { error });
        var player = new Player(body.FirstName.Trim(), body.LastName.Trim(), Slugify(body.Slug), body.DateOfBirth, body.CountryId);
        player.AssignClub(body.ClubId);
        player.SetProfile(body.PrimaryPosition.Trim().ToUpperInvariant(), body.SecondaryPositions?.Trim() ?? "", body.PreferredFoot, body.HeightCm, body.Personality?.Trim() ?? "", body.MediaDescription?.Trim() ?? "");
        player.SetValuation(body.CurrentAbility, body.PotentialAbility, body.MarketValue, body.WeeklyWage, body.ContractExpiresAt);
        player.SetFeatured(body.IsFeatured);
        db.Players.Add(player); AddAudit("create", "Player", player.Slug, $"{player.FullName} created");
        await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Players), new { id = player.Id }, new { player.Id, player.Slug });
    }

    [HttpPut("players/{id:guid}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] PlayerUpsertRequest body, CancellationToken ct)
    {
        var player = await db.Players.FirstOrDefaultAsync(x => x.Id == id, ct); if (player is null) return NotFound();
        var error = await ValidatePlayer(body, id, ct); if (error is not null) return BadRequest(new { error });
        player.UpdateIdentity(body.FirstName.Trim(), body.LastName.Trim(), Slugify(body.Slug), body.DateOfBirth, body.CountryId);
        player.AssignClub(body.ClubId);
        player.SetProfile(body.PrimaryPosition.Trim().ToUpperInvariant(), body.SecondaryPositions?.Trim() ?? "", body.PreferredFoot, body.HeightCm, body.Personality?.Trim() ?? "", body.MediaDescription?.Trim() ?? "");
        player.SetValuation(body.CurrentAbility, body.PotentialAbility, body.MarketValue, body.WeeklyWage, body.ContractExpiresAt);
        player.SetFeatured(body.IsFeatured); AddAudit("update", "Player", player.Slug, $"{player.FullName} updated");
        await db.SaveChangesAsync(ct); return Ok(new { player.Id, player.Slug });
    }

    [HttpDelete("players/{id:guid}")]
    public async Task<IActionResult> DeletePlayer(Guid id, CancellationToken ct)
    {
        var player = await db.Players.FirstOrDefaultAsync(x => x.Id == id, ct); if (player is null) return NotFound();
        db.Players.Remove(player); AddAudit("delete", "Player", player.Slug, player.FullName); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpPost("players/import-csv")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> ImportPlayers(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0 || !file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { error = "A non-empty CSV file is required." });
        var countries = await db.Countries.AsNoTracking().ToDictionaryAsync(x => x.Code.ToUpperInvariant(), ct);
        var clubs = await db.Clubs.AsNoTracking().ToDictionaryAsync(x => x.Slug, ct);
        var existing = await db.Players.AsNoTracking().Select(x => x.Slug).ToHashSetAsync(ct);
        var imported = 0; var skipped = 0; var errors = new List<object>();
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
        var header = await reader.ReadLineAsync(ct);
        if (header is null) return BadRequest(new { error = "CSV header is missing." });
        var row = 1;
        while (!reader.EndOfStream)
        {
            row++; var line = await reader.ReadLineAsync(ct); if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var c = ParseCsvLine(line); if (c.Count < 12) throw new InvalidDataException("Expected at least 12 columns.");
                var slug = Slugify(c[2]); if (existing.Contains(slug)) { skipped++; continue; }
                if (!countries.TryGetValue(c[4].ToUpperInvariant(), out var country)) throw new InvalidDataException($"Unknown country code: {c[4]}");
                Guid? clubId = null; if (!string.IsNullOrWhiteSpace(c[5])) { if (!clubs.TryGetValue(c[5], out var club)) throw new InvalidDataException($"Unknown club slug: {c[5]}"); clubId = club.Id; }
                var player = new Player(c[0], c[1], slug, DateOnly.Parse(c[3], CultureInfo.InvariantCulture), country.Id);
                player.AssignClub(clubId); player.SetProfile(c[6].ToUpperInvariant(), c.Count > 12 ? c[12] : "", Enum.Parse<PreferredFoot>(c[7], true), int.Parse(c[8], CultureInfo.InvariantCulture), c.Count > 13 ? c[13] : "", c.Count > 14 ? c[14] : "");
                player.SetValuation(int.Parse(c[9], CultureInfo.InvariantCulture), int.Parse(c[10], CultureInfo.InvariantCulture), decimal.Parse(c[11], CultureInfo.InvariantCulture), c.Count > 15 ? decimal.Parse(c[15], CultureInfo.InvariantCulture) : 0, c.Count > 16 && DateOnly.TryParse(c[16], out var expiry) ? expiry : null);
                db.Players.Add(player); existing.Add(slug); imported++;
            }
            catch (Exception ex) { errors.Add(new { row, error = ex.Message }); if (errors.Count >= 50) break; }
        }
        AddAudit("import", "Player", file.FileName, $"Imported={imported}; skipped={skipped}; errors={errors.Count}"); await db.SaveChangesAsync(ct);
        return Ok(new { imported, skipped, errors });
    }

    [HttpPost("articles")] public async Task<IActionResult> CreateArticle([FromBody] Article body, CancellationToken ct) { body.Slug = Slugify(body.Slug); body.IsPublished = true; body.PublishedAt = DateTimeOffset.UtcNow; db.Articles.Add(body); AddAudit("create", "Article", body.Slug, body.Title); await db.SaveChangesAsync(ct); return Ok(body); }
    [HttpPut("articles/{id:guid}")] public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] Article body, CancellationToken ct) { var x = await db.Articles.FindAsync([id], ct); if (x is null) return NotFound(); x.Title = body.Title; x.Slug = Slugify(body.Slug); x.Summary = body.Summary; x.Body = body.Body; x.Category = body.Category; x.Tags = body.Tags; x.IsPublished = body.IsPublished; x.PublishedAt = body.IsPublished ? x.PublishedAt ?? DateTimeOffset.UtcNow : null; AddAudit("update", "Article", x.Slug, x.Title); await db.SaveChangesAsync(ct); return Ok(x); }
    [HttpDelete("articles/{id:guid}")] public async Task<IActionResult> DeleteArticle(Guid id, CancellationToken ct) { var x = await db.Articles.FindAsync([id], ct); if (x is null) return NotFound(); db.Articles.Remove(x); AddAudit("delete", "Article", x.Slug, x.Title); await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpPost("collections")] public async Task<IActionResult> CreateCollection([FromBody] PlayerCollection body, CancellationToken ct) { body.Slug = Slugify(body.Slug); db.PlayerCollections.Add(body); AddAudit("create", "PlayerCollection", body.Slug, body.Name); await db.SaveChangesAsync(ct); return Ok(body); }

    private async Task<string?> ValidatePlayer(PlayerUpsertRequest x, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(x.FirstName) || string.IsNullOrWhiteSpace(x.LastName)) return "First and last name are required.";
        if (x.CurrentAbility is < 1 or > 200 || x.PotentialAbility is < 1 or > 200 || x.PotentialAbility < x.CurrentAbility) return "Ability values must be 1-200 and potential cannot be below current ability.";
        if (x.MarketValue < 0 || x.WeeklyWage < 0 || x.HeightCm is < 140 or > 220) return "Invalid financial or physical values.";
        if (!await db.Countries.AnyAsync(c => c.Id == x.CountryId, ct)) return "Country does not exist.";
        if (x.ClubId.HasValue && !await db.Clubs.AnyAsync(c => c.Id == x.ClubId, ct)) return "Club does not exist.";
        var slug = Slugify(x.Slug); if (await db.Players.AnyAsync(p => p.Slug == slug && p.Id != id, ct)) return "Slug is already in use.";
        return null;
    }
    private void AddAudit(string action, string entity, string id, string details) => db.AuditLogs.Add(new AuditLog { Action = action, EntityName = entity, EntityId = id, Actor = "admin", Details = details });
    private static string Slugify(string input) => string.Join('-', input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD).Where(c => char.IsLetterOrDigit(c) || c is ' ' or '-').Select(c => c == ' ' ? '-' : c)).Replace("--", "-");
    private static List<string> ParseCsvLine(string line) { var result = new List<string>(); var current = new StringBuilder(); var quoted = false; for (var i = 0; i < line.Length; i++) { var ch = line[i]; if (ch == '"') { if (quoted && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; } else quoted = !quoted; } else if (ch == ',' && !quoted) { result.Add(current.ToString().Trim()); current.Clear(); } else current.Append(ch); } result.Add(current.ToString().Trim()); return result; }
}

public sealed record PlayerUpsertRequest(string FirstName, string LastName, string Slug, DateOnly DateOfBirth, Guid CountryId, Guid? ClubId, string PrimaryPosition, string? SecondaryPositions, PreferredFoot PreferredFoot, int HeightCm, string? Personality, string? MediaDescription, int CurrentAbility, int PotentialAbility, decimal MarketValue, decimal WeeklyWage, DateOnly? ContractExpiresAt, bool IsFeatured);

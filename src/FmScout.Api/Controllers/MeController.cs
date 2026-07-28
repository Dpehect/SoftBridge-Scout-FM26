using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace FmScout.Api.Controllers;
[ApiController, Route("api/me"), Authorize]
public sealed class MeController(FmScoutDbContext db) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    [HttpGet("favorites")]
    public async Task<IActionResult> Favorites(CancellationToken ct) => Ok(await db.FavoritePlayers.Where(x => x.UserAccountId == UserId).Select(x => new { x.Player.Id, x.Player.Slug, name = x.Player.FirstName + " " + x.Player.LastName, x.Player.PrimaryPosition, x.Player.PotentialAbility }).ToListAsync(ct));
    [HttpPost("favorites/{playerId:guid}")]
    public async Task<IActionResult> Add(Guid playerId, CancellationToken ct) { if (!await db.FavoritePlayers.AnyAsync(x => x.UserAccountId == UserId && x.PlayerId == playerId, ct)) { db.FavoritePlayers.Add(new FavoritePlayer { UserAccountId = UserId, PlayerId = playerId }); await db.SaveChangesAsync(ct); } return NoContent(); }
    [HttpDelete("favorites/{playerId:guid}")]
    public async Task<IActionResult> Remove(Guid playerId, CancellationToken ct) { var row = await db.FavoritePlayers.SingleOrDefaultAsync(x => x.UserAccountId == UserId && x.PlayerId == playerId, ct); if (row is not null) { db.Remove(row); await db.SaveChangesAsync(ct); } return NoContent(); }
}

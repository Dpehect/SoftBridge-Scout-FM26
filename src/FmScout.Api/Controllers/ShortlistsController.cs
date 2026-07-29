using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController,Route("api/shortlists")]
public sealed class ShortlistsController(FmScoutDbContext db):ControllerBase
{
 static string Owner(HttpRequest r)=>r.Headers["X-Owner-Key"].FirstOrDefault()??"demo";
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct){var owner=Owner(Request);return Ok(await db.Shortlists.AsNoTracking().Where(x=>x.OwnerKey==owner).Include(x=>x.Players).ThenInclude(x=>x.Player).ToListAsync(ct));}
 [HttpPost] public async Task<IActionResult> Create([FromBody]CreateShortlist body,CancellationToken ct){var x=new Shortlist{OwnerKey=Owner(Request),Name=body.Name.Trim(),IsPublic=body.IsPublic};db.Shortlists.Add(x);await db.SaveChangesAsync(ct);return Created($"api/shortlists/{x.Id}",x);}
 [HttpPost("{id:guid}/players/{playerId:guid}")] public async Task<IActionResult> Add(Guid id,Guid playerId,[FromBody]AddPlayer body,CancellationToken ct){var owner=Owner(Request);if(!await db.Shortlists.AnyAsync(x=>x.Id==id&&x.OwnerKey==owner,ct))return NotFound();if(!await db.ShortlistPlayers.AnyAsync(x=>x.ShortlistId==id&&x.PlayerId==playerId,ct)){db.ShortlistPlayers.Add(new ShortlistPlayer{ShortlistId=id,PlayerId=playerId,Note=body.Note??""});await db.SaveChangesAsync(ct);}return NoContent();}
 [HttpDelete("{id:guid}/players/{playerId:guid}")] public async Task<IActionResult> Remove(Guid id,Guid playerId,CancellationToken ct){var x=await db.ShortlistPlayers.Include(x=>x.Shortlist).SingleOrDefaultAsync(x=>x.ShortlistId==id&&x.PlayerId==playerId&&x.Shortlist.OwnerKey==Owner(Request),ct);if(x is null)return NotFound();db.Remove(x);await db.SaveChangesAsync(ct);return NoContent();}
 public sealed record CreateShortlist(string Name,bool IsPublic); public sealed record AddPlayer(string? Note);
}

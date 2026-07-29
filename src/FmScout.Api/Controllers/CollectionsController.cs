using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController,Route("api/collections")]
public sealed class CollectionsController(FmScoutDbContext db):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.PlayerCollections.AsNoTracking().OrderByDescending(x=>x.IsFeatured).ThenBy(x=>x.Name).ToListAsync(ct));
 [HttpGet("{slug}")] public async Task<IActionResult> Detail(string slug,CancellationToken ct)
 {
  var c=await db.PlayerCollections.AsNoTracking().SingleOrDefaultAsync(x=>x.Slug==slug,ct); if(c is null)return NotFound();
  var q=db.Players.AsNoTracking().Include(x=>x.Country).Include(x=>x.Club).AsQueryable();
  q=c.RuleKey switch {"wonderkids"=>q.Where(x=>x.IsWonderkid),"free-agents"=>q.Where(x=>x.ClubId==null),"expiring"=>q.Where(x=>x.ContractExpiresAt!=null&&x.ContractExpiresAt<=DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))),"hidden-gems"=>q.Where(x=>x.PotentialAbility>=150&&x.MarketValue<=5_000_000),_=>q.Where(x=>x.IsFeatured)};
  return Ok(new{collection=c,players=await q.OrderByDescending(x=>x.PotentialAbility).Take(48).Select(x=>new{x.Id,x.Slug,x.FullName,x.Age,x.PrimaryPosition,country=x.Country.Name,club=x.Club==null?null:x.Club.Name,x.MarketValue,x.CurrentAbility,x.PotentialAbility,x.IsWonderkid}).ToListAsync(ct)});
 }
}

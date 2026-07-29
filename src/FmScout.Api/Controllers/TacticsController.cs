using System.Text.Json;
using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController,Route("api/tactics")]
public sealed class TacticsController(FmScoutDbContext db):ControllerBase
{
 static string Owner(HttpRequest r)=>r.Headers["X-Owner-Key"].FirstOrDefault()??"demo";
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.Tactics.AsNoTracking().Where(x=>x.OwnerKey==Owner(Request)).ToListAsync(ct));
 [HttpPost] public async Task<IActionResult> Save([FromBody]SaveTactic body,CancellationToken ct){var x=new Tactic{OwnerKey=Owner(Request),Name=body.Name,Formation=body.Formation,Mentality=body.Mentality,RolesJson=JsonSerializer.Serialize(body.Roles)};db.Tactics.Add(x);await db.SaveChangesAsync(ct);return Ok(x);}
 [HttpPost("analyze")] public async Task<IActionResult> Analyze([FromBody]AnalyzeTactic body,CancellationToken ct){var positions=body.Roles.Select(x=>x.Position).Distinct().ToArray();var players=await db.Players.AsNoTracking().Include(x=>x.Country).Where(x=>positions.Contains(x.PrimaryPosition)).OrderByDescending(x=>x.PotentialAbility+x.CurrentAbility).Take(30).Select(x=>new{x.Slug,x.FullName,x.PrimaryPosition,x.CurrentAbility,x.PotentialAbility,x.MarketValue,country=x.Country.Name}).ToListAsync(ct);return Ok(new{body.Formation,coverage=positions.Length,recommendations=players});}
 public sealed record RoleSlot(string Position,string Role); public sealed record SaveTactic(string Name,string Formation,string Mentality,RoleSlot[] Roles); public sealed record AnalyzeTactic(string Formation,RoleSlot[] Roles);
}

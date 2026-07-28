using FmScout.Api.Security;
using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController,Route("api/admin"),AdminKey]
public sealed class AdminController(FmScoutDbContext db):ControllerBase
{
 [HttpGet("dashboard")] public async Task<IActionResult> Dashboard(CancellationToken ct)=>Ok(new{players=await db.Players.CountAsync(ct),clubs=await db.Clubs.CountAsync(ct),articles=await db.Articles.CountAsync(ct),shortlists=await db.Shortlists.CountAsync(ct),audit=await db.AuditLogs.OrderByDescending(x=>x.CreatedAt).Take(20).ToListAsync(ct)});
 [HttpPost("articles")] public async Task<IActionResult> CreateArticle([FromBody]Article body,CancellationToken ct){body.IsPublished=true;body.PublishedAt=DateTimeOffset.UtcNow;db.Articles.Add(body);db.AuditLogs.Add(new AuditLog{Action="create",EntityName="Article",EntityId=body.Slug,Actor="admin"});await db.SaveChangesAsync(ct);return Ok(body);}
 [HttpPost("collections")] public async Task<IActionResult> CreateCollection([FromBody]PlayerCollection body,CancellationToken ct){db.PlayerCollections.Add(body);db.AuditLogs.Add(new AuditLog{Action="create",EntityName="PlayerCollection",EntityId=body.Slug,Actor="admin"});await db.SaveChangesAsync(ct);return Ok(body);}
 [HttpDelete("articles/{id:guid}")] public async Task<IActionResult> DeleteArticle(Guid id,CancellationToken ct){var x=await db.Articles.FindAsync([id],ct);if(x is null)return NotFound();db.Articles.Remove(x);await db.SaveChangesAsync(ct);return NoContent();}
}

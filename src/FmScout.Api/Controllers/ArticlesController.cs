using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController,Route("api/articles")]
public sealed class ArticlesController(FmScoutDbContext db):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.Articles.AsNoTracking().Where(x=>x.IsPublished).OrderByDescending(x=>x.PublishedAt).Select(x=>new{x.Title,x.Slug,x.Summary,x.Category,x.Tags,x.PublishedAt}).ToListAsync(ct));
 [HttpGet("{slug}")] public async Task<IActionResult> Detail(string slug,CancellationToken ct){var x=await db.Articles.AsNoTracking().SingleOrDefaultAsync(x=>x.Slug==slug&&x.IsPublished,ct);return x is null?NotFound():Ok(x);}
}

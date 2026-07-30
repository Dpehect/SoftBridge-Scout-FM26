using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController,Route("api/scout-reports")]
public sealed class ScoutReportsController(FmScoutDbContext db):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken ct)=>Ok(await db.ScoutReports.AsNoTracking().Include(x=>x.Player).OrderByDescending(x=>x.CreatedAt).Take(250).ToListAsync(ct));
    [HttpPost]
    public async Task<ActionResult> Create(ScoutReport report,CancellationToken ct){report.Id=Guid.NewGuid();report.CreatedAt=DateTimeOffset.UtcNow;db.ScoutReports.Add(report);await db.SaveChangesAsync(ct);return Created($"/api/scout-reports/{report.Id}",report);}
}

using FmScout.Application.Players;
using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController, Route("api/players")]
public sealed class PlayersController(FmScoutDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlayerDto>>> Get([FromQuery]string? search,[FromQuery]bool? wonderkid,[FromQuery]bool? hiddenGem,CancellationToken ct)
    {
        var q=db.Players.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.Name.ToLower().Contains(search.ToLower())||x.Club.ToLower().Contains(search.ToLower()));
        if(wonderkid.HasValue) q=q.Where(x=>x.IsWonderkid==wonderkid.Value);
        if(hiddenGem.HasValue) q=q.Where(x=>x.IsHiddenGem==hiddenGem.Value);
        var data=await q.OrderByDescending(x=>x.PotentialAbility).Take(250).Select(x=>new PlayerDto(x.Id,x.Name,x.Slug,x.Age,x.Position,x.Club,x.Nation,x.CurrentAbility,x.PotentialAbility,x.MarketValue,x.IsWonderkid,x.IsHiddenGem)).ToListAsync(ct);
        return Ok(data);
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<PlayerDto>> GetByKey(string key,CancellationToken ct)
    {
        Player? player;
        if(Guid.TryParse(key,out var id))
            player=await db.Players.AsNoTracking().FirstOrDefaultAsync(p=>p.Id==id,ct);
        else
            player=await db.Players.AsNoTracking().FirstOrDefaultAsync(p=>p.Slug==key,ct);

        if(player is null)return NotFound();
        return Ok(new PlayerDto(player.Id,player.Name,player.Slug,player.Age,player.Position,player.Club,player.Nation,player.CurrentAbility,player.PotentialAbility,player.MarketValue,player.IsWonderkid,player.IsHiddenGem));
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Create(CreatePlayerRequest r,CancellationToken ct)
    {
        var slug=string.Join('-',r.Name.Trim().ToLowerInvariant().Split(' ',StringSplitOptions.RemoveEmptyEntries));
        var p=new Player{Name=r.Name.Trim(),Slug=$"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length+9,slug.Length+9)],Age=r.Age,Position=r.Position,Club=r.Club,Nation=r.Nation,CurrentAbility=r.CurrentAbility,PotentialAbility=r.PotentialAbility,MarketValue=r.MarketValue,IsWonderkid=r.IsWonderkid,IsHiddenGem=r.IsHiddenGem};
        db.Players.Add(p); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetByKey),new{key=p.Id},new PlayerDto(p.Id,p.Name,p.Slug,p.Age,p.Position,p.Club,p.Nation,p.CurrentAbility,p.PotentialAbility,p.MarketValue,p.IsWonderkid,p.IsHiddenGem));
    }
}
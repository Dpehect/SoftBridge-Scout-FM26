using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Infrastructure.Seed;
public static class DatabaseSeeder
{
    public static async Task SeedAsync(FmScoutDbContext db, CancellationToken ct=default)
    {
        await db.Database.EnsureCreatedAsync(ct);
        if (await db.Players.AnyAsync(ct)) return;
        db.Players.AddRange(
            new Player{Name="Elite Wonderkid",Slug="elite-wonderkid",Age=18,Position="AM",Club="Example FC",Nation="Türkiye",CurrentAbility=125,PotentialAbility=178,MarketValue=12_000_000,IsWonderkid=true},
            new Player{Name="Hidden Gem",Slug="hidden-gem",Age=20,Position="DM",Club="Scout United",Nation="Portugal",CurrentAbility=118,PotentialAbility=165,MarketValue=4_500_000,IsHiddenGem=true}
        );
        await db.SaveChangesAsync(ct);
    }
}

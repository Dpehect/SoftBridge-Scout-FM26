using FmScout.Domain.Entities;
using FmScout.Domain.Enums;
using FmScout.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Infrastructure.Seed;
public static class DatabaseSeeder
{
    public static async Task SeedAsync(FmScoutDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct); if (await db.Players.AnyAsync(ct)) return;
        var tr = new Country("Türkiye", "TUR"); var es = new Country("Spain", "ESP"); var de = new Country("Germany", "DEU"); var br = new Country("Brazil", "BRA"); var fr = new Country("France", "FRA");
        db.Countries.AddRange(tr,es,de,br,fr); await db.SaveChangesAsync(ct);
        var clubs = new[] { new Club("İstanbul Athletic","istanbul-athletic","IST",tr.Id), new Club("Madrid Unión","madrid-union","MAD",es.Id), new Club("Berlin 04","berlin-04","B04",de.Id), new Club("São Paulo Azul","sao-paulo-azul","SPA",br.Id), new Club("Paris Métropole","paris-metropole","PAR",fr.Id) };
        db.Clubs.AddRange(clubs);
        var roles = new[] { new TacticalRole("Advanced Forward","advanced-forward","ST"), new TacticalRole("Inside Forward","inside-forward","AML"), new TacticalRole("Deep-Lying Playmaker","deep-lying-playmaker","CM"), new TacticalRole("Ball-Playing Defender","ball-playing-defender","CB"), new TacticalRole("Sweeper Keeper","sweeper-keeper","GK") };
        db.TacticalRoles.AddRange(roles); await db.SaveChangesAsync(ct);
        var data = new[] {
            ("Arda","Demir","arda-demir",new DateOnly(2007,3,12),tr,clubs[0],"AMR",170,188,18_500_000m,42_000m),
            ("Mateo","Vidal","mateo-vidal",new DateOnly(2006,8,2),es,clubs[1],"CM",164,184,24_000_000m,51_000m),
            ("Lukas","Brandt","lukas-brandt",new DateOnly(2005,11,22),de,clubs[2],"CB",160,181,19_000_000m,47_000m),
            ("João","Ribeiro","joao-ribeiro",new DateOnly(2007,5,8),br,clubs[3],"ST",158,190,16_000_000m,32_000m),
            ("Noah","Laurent","noah-laurent",new DateOnly(2006,1,15),fr,clubs[4],"AML",167,186,27_500_000m,58_000m),
            ("Emir","Kaya","emir-kaya",new DateOnly(2004,9,9),tr,clubs[0],"GK",154,176,11_000_000m,29_000m),
            ("Diego","Santos","diego-santos",new DateOnly(2003,4,19),br,clubs[3],"DM",159,174,13_500_000m,35_000m),
            ("Hugo","Martin","hugo-martin",new DateOnly(2002,12,3),fr,clubs[4],"RB",163,172,21_000_000m,44_000m)
        };
        var rng = new Random(26); var players = new List<Player>();
        foreach (var d in data) {
            var p = new Player(d.Item1,d.Item2,d.Item3,d.Item4,d.Item5.Id); p.AssignClub(d.Item6.Id); p.SetProfile(d.Item7,"CM,AM",PreferredFoot.Right,rng.Next(174,193),"Driven","Promising talent"); p.SetValuation(d.Item8,d.Item9,d.Item10,d.Item11,new DateOnly(2029,6,30)); p.SetFeatured(rng.NextDouble()>.45);
            var a = new PlayerAttributes(p.Id) { Acceleration=rng.Next(12,19),Pace=rng.Next(12,19),Stamina=rng.Next(11,19),Strength=rng.Next(9,18),Passing=rng.Next(11,19),Technique=rng.Next(12,19),FirstTouch=rng.Next(11,19),Dribbling=rng.Next(10,19),Finishing=rng.Next(8,19),Tackling=rng.Next(7,18),Marking=rng.Next(7,18),Crossing=rng.Next(8,18),Decisions=rng.Next(11,19),Vision=rng.Next(10,19),WorkRate=rng.Next(11,20),Teamwork=rng.Next(10,19),Anticipation=rng.Next(11,19),Composure=rng.Next(11,19),Determination=rng.Next(13,20),Positioning=rng.Next(8,18)};
            db.Players.Add(p); db.PlayerAttributes.Add(a); players.Add(p);
        }
        await db.SaveChangesAsync(ct);
        foreach (var p in players) foreach (var role in roles) db.PlayerRoleScores.Add(new PlayerRoleScore(p.Id,role.Id,Math.Round((decimal)(rng.NextDouble()*22+70),2)));
        await db.SaveChangesAsync(ct);
    }
}

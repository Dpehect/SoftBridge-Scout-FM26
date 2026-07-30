using FmScout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Infrastructure.Persistence;
public sealed class FmScoutDbContext(DbContextOptions<FmScoutDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<ScoutReport> ScoutReports => Set<ScoutReport>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(e => { e.ToTable("players"); e.HasKey(x=>x.Id); e.HasIndex(x=>x.Slug).IsUnique(); e.Property(x=>x.Name).HasMaxLength(160); e.Property(x=>x.Slug).HasMaxLength(180); e.Property(x=>x.MarketValue).HasPrecision(18,2); });
        modelBuilder.Entity<ScoutReport>(e => { e.ToTable("scout_reports"); e.HasKey(x=>x.Id); e.HasOne(x=>x.Player).WithMany().HasForeignKey(x=>x.PlayerId).OnDelete(DeleteBehavior.Cascade); });
    }
}

using FmScout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Infrastructure.Persistence;
public sealed class FmScoutDbContext(DbContextOptions<FmScoutDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>(); public DbSet<PlayerAttributes> PlayerAttributes => Set<PlayerAttributes>();
    public DbSet<PlayerRoleScore> PlayerRoleScores => Set<PlayerRoleScore>(); public DbSet<TacticalRole> TacticalRoles => Set<TacticalRole>();
    public DbSet<Article> Articles => Set<Article>(); public DbSet<PlayerCollection> PlayerCollections => Set<PlayerCollection>(); public DbSet<Shortlist> Shortlists => Set<Shortlist>(); public DbSet<ShortlistPlayer> ShortlistPlayers => Set<ShortlistPlayer>(); public DbSet<Tactic> Tactics => Set<Tactic>(); public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>(); public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>(); public DbSet<FavoritePlayer> FavoritePlayers => Set<FavoritePlayer>();
    public DbSet<Club> Clubs => Set<Club>(); public DbSet<Competition> Competitions => Set<Competition>(); public DbSet<Country> Countries => Set<Country>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("scout");
        modelBuilder.Entity<Country>(b => { b.HasIndex(x => x.Code).IsUnique(); b.Property(x => x.Name).HasMaxLength(100); b.Property(x => x.Code).HasMaxLength(3); });
        modelBuilder.Entity<Competition>(b => { b.HasIndex(x => x.Slug).IsUnique(); b.Property(x => x.Name).HasMaxLength(150); b.Property(x => x.Slug).HasMaxLength(160); });
        modelBuilder.Entity<Club>(b => { b.HasIndex(x => x.Slug).IsUnique(); b.Property(x => x.Name).HasMaxLength(150); b.Property(x => x.Slug).HasMaxLength(160); b.Property(x => x.ShortName).HasMaxLength(40); });
        modelBuilder.Entity<TacticalRole>(b => { b.HasIndex(x => x.Slug).IsUnique(); b.Property(x => x.Name).HasMaxLength(100); b.Property(x => x.PositionCode).HasMaxLength(12); });
        modelBuilder.Entity<Player>(b => {
            b.HasIndex(x => x.Slug).IsUnique(); b.HasIndex(x => new { x.PotentialAbility, x.CurrentAbility }); b.HasIndex(x => new { x.PrimaryPosition, x.MarketValue });
            b.Property(x => x.FirstName).HasMaxLength(80); b.Property(x => x.LastName).HasMaxLength(80); b.Property(x => x.Slug).HasMaxLength(180);
            b.Property(x => x.MarketValue).HasPrecision(14, 2); b.Property(x => x.WeeklyWage).HasPrecision(12, 2);
            b.HasOne(x => x.Attributes).WithOne(x => x.Player).HasForeignKey<PlayerAttributes>(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Article>(b=>{b.HasIndex(x=>x.Slug).IsUnique();b.Property(x=>x.Title).HasMaxLength(180);b.Property(x=>x.Slug).HasMaxLength(200);});
        modelBuilder.Entity<PlayerCollection>(b=>{b.HasIndex(x=>x.Slug).IsUnique();b.Property(x=>x.Name).HasMaxLength(140);});
        modelBuilder.Entity<Shortlist>(b=>{b.HasIndex(x=>x.ShareCode).IsUnique();b.Property(x=>x.Name).HasMaxLength(120);});
        modelBuilder.Entity<ShortlistPlayer>(b=>{b.HasIndex(x=>new{x.ShortlistId,x.PlayerId}).IsUnique();});
        modelBuilder.Entity<Tactic>(b=>{b.Property(x=>x.Name).HasMaxLength(120);b.Property(x=>x.RolesJson).HasColumnType("jsonb");});
        modelBuilder.Entity<UserAccount>(b => { b.HasIndex(x => x.Email).IsUnique(); b.Property(x => x.Email).HasMaxLength(180); b.Property(x => x.DisplayName).HasMaxLength(100); b.Property(x => x.Role).HasMaxLength(30); });
        modelBuilder.Entity<RefreshToken>(b => { b.HasIndex(x => x.TokenHash).IsUnique(); b.HasIndex(x => new { x.UserAccountId, x.ExpiresAt }); });
        modelBuilder.Entity<FavoritePlayer>(b => { b.HasIndex(x => new { x.UserAccountId, x.PlayerId }).IsUnique(); });
        modelBuilder.Entity<PlayerRoleScore>(b => { b.HasIndex(x => new { x.PlayerId, x.TacticalRoleId }).IsUnique(); b.Property(x => x.Score).HasPrecision(5,2); });
    }
}

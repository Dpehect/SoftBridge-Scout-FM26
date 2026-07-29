using FmScout.Application.Abstractions;
using FmScout.Infrastructure.Persistence;
using FmScout.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace FmScout.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
        var connectionString = NormalizePostgresConnectionString(rawConnectionString);
        services.AddDbContextPool<FmScoutDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
            npgsql.EnableRetryOnFailure(4, TimeSpan.FromSeconds(5), null)));
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IScoutingRepository, ScoutingRepository>();
        return services;
    }

    private static string NormalizePostgresConnectionString(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return value;
        }

        var credentials = uri.UserInfo.Split(':', 2);
        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty,
            SslMode = Npgsql.SslMode.Require,
            Pooling = true,
            MaxPoolSize = 20,
            Timeout = 15,
            CommandTimeout = 30
        };

        return builder.ConnectionString;
    }
}

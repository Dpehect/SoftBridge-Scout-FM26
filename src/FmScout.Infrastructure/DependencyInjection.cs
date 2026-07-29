using FmScout.Application.Abstractions;
using FmScout.Infrastructure.Persistence;
using FmScout.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FmScout.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rawConnectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"]
            ?? configuration["POSTGRES_URL"];

        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL connection string is missing. " +
                "Set Render environment variable ConnectionStrings__DefaultConnection " +
                "or DATABASE_URL to the Neon connection string.");
        }

        var connectionString = NormalizePostgresConnectionString(rawConnectionString);

        services.AddDbContextPool<FmScoutDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 6,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                npgsql.CommandTimeout(60);
            }));

        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IScoutingRepository, ScoutingRepository>();

        return services;
    }

    private static string NormalizePostgresConnectionString(string value)
    {
        value = value.Trim();

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            var builder = new NpgsqlConnectionStringBuilder(value)
            {
                SslMode = SslMode.Require,
                Pooling = true,
                MaxPoolSize = 20,
                Timeout = 30,
                CommandTimeout = 60,
                KeepAlive = 30
            };

            return builder.ConnectionString;
        }

        var credentials = uri.UserInfo.Split(':', 2);

        if (credentials.Length == 0 || string.IsNullOrWhiteSpace(credentials[0]))
        {
            throw new InvalidOperationException("The PostgreSQL URL does not contain a username.");
        }

        var connectionBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1
                ? Uri.UnescapeDataString(credentials[1])
                : string.Empty,
            SslMode = SslMode.Require,
            Pooling = true,
            MaxPoolSize = 20,
            Timeout = 30,
            CommandTimeout = 60,
            KeepAlive = 30
        };

        return connectionBuilder.ConnectionString;
    }
}

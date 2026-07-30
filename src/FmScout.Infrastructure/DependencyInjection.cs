using FmScout.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
namespace FmScout.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(raw)) return services;
        var connectionString = Normalize(raw);
        services.AddDbContextPool<FmScoutDbContext>(options => options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(4, TimeSpan.FromSeconds(5), null)));
        return services;
    }
    private static string Normalize(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != "postgres" && uri.Scheme != "postgresql")) return value;
        var credentials = uri.UserInfo.Split(':',2);
        return new NpgsqlConnectionStringBuilder { Host=uri.Host, Port=uri.IsDefaultPort?5432:uri.Port, Database=uri.AbsolutePath.TrimStart('/'), Username=Uri.UnescapeDataString(credentials[0]), Password=credentials.Length>1?Uri.UnescapeDataString(credentials[1]):string.Empty, SslMode=SslMode.Require, Pooling=true, MaxPoolSize=20, Timeout=15, CommandTimeout=30 }.ConnectionString;
    }
}

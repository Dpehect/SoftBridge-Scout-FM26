using FmScout.Application.Abstractions;
namespace FmScout.Application.Scouting;
public sealed class ScoutingService(IScoutingRepository repository)
{
    public Task<IReadOnlyList<ScoutRecommendationDto>> RecommendAsync(ScoutRequest request, CancellationToken ct)
        => repository.RecommendAsync(request with { Limit = Math.Clamp(request.Limit, 1, 30) }, ct);

    public Task<PlayerComparisonDto> CompareAsync(IReadOnlyCollection<string> slugs, CancellationToken ct)
    {
        var clean = slugs.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToArray();
        if (clean.Length < 2) throw new ArgumentException("Karşılaştırma için en az iki oyuncu gerekir.");
        return repository.CompareAsync(clean, ct);
    }

    public Task<PlatformStatsDto> GetStatsAsync(CancellationToken ct) => repository.GetStatsAsync(ct);
}

using FmScout.Application.Abstractions;
using FmScout.Application.Common;
namespace FmScout.Application.Players;
public sealed class PlayerService(IPlayerRepository repository)
{
    public Task<PagedResult<PlayerListItemDto>> SearchAsync(PlayerSearchQuery query, CancellationToken cancellationToken)
    {
        var normalized = query with { Page = Math.Max(1, query.Page), PageSize = Math.Clamp(query.PageSize, 1, 100) };
        return repository.SearchAsync(normalized, cancellationToken);
    }
    public async Task<PlayerDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => await repository.GetBySlugAsync(slug, cancellationToken) ?? throw new KeyNotFoundException("Player not found.");
}

using FmScout.Application.Common;
using FmScout.Application.Players;
namespace FmScout.Application.Abstractions;
public interface IPlayerRepository
{
    Task<PagedResult<PlayerListItemDto>> SearchAsync(PlayerSearchQuery query, CancellationToken cancellationToken);
    Task<PlayerDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}

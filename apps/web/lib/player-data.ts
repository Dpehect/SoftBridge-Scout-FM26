import rawPlayers from "@/data/players/top5.json";
import type { Player, PagedPlayers } from "@/lib/api";

const players = rawPlayers as Player[];

function includes(value: string | null | undefined, query: string) {
  return (value ?? "").toLocaleLowerCase("tr-TR").includes(query.toLocaleLowerCase("tr-TR"));
}

export function getJsonPlayers(params: Record<string, string | undefined> = {}): PagedPlayers {
  const page = Math.max(1, Number(params.page ?? 1));
  const pageSize = Math.min(100, Math.max(1, Number(params.pageSize ?? 24)));
  const search = params.search?.trim() ?? params.q?.trim() ?? "";
  const club = params.club?.trim() ?? "";
  const country = params.country?.trim() ?? "";
  const position = params.position?.trim() ?? "";
  const league = params.league?.trim() ?? "";
  const wonderkidsOnly = params.isWonderkid === "true" || params.wonderkids === "true";
  const featuredOnly = params.isFeatured === "true" || params.hiddenGems === "true";

  let filtered = players.filter((player) => {
    if (search && ![player.fullName, player.club, player.country, player.position].some((value) => includes(value, search))) return false;
    if (club && !includes(player.club, club)) return false;
    if (country && !includes(player.country, country)) return false;
    if (position && !includes(player.position, position)) return false;
    if (league && !includes((player as Player & { league?: string }).league, league)) return false;
    if (wonderkidsOnly && !player.isWonderkid) return false;
    if (featuredOnly && !player.isFeatured) return false;
    return true;
  });

  const sort = params.sort ?? "potentialAbility";
  const direction = params.direction === "asc" ? 1 : -1;
  filtered = [...filtered].sort((a, b) => {
    const av = Number((a as unknown as Record<string, unknown>)[sort] ?? 0);
    const bv = Number((b as unknown as Record<string, unknown>)[sort] ?? 0);
    return (av - bv) * direction;
  });

  const totalCount = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const start = (page - 1) * pageSize;
  return { items: filtered.slice(start, start + pageSize), page, pageSize, totalCount, totalPages };
}

export function getJsonPlayer(slug: string): Player | null {
  return players.find((player) => player.slug === slug || player.id === slug) ?? null;
}

export function getJsonStats() {
  return {
    players: players.length,
    clubs: new Set(players.map((player) => player.club).filter(Boolean)).size,
    countries: new Set(players.map((player) => player.country).filter(Boolean)).size,
    roles: new Set(players.map((player) => player.position).filter(Boolean)).size,
    wonderkids: players.filter((player) => player.isWonderkid).length
  };
}

import rawPlayers from "@/data/players/top5.json";
import type { Player, PagedPlayers } from "@/lib/api";

const players = rawPlayers as Player[];

function includes(value: string | null | undefined, query: string) {
  return (value ?? "").toLocaleLowerCase("tr-TR").includes(query.toLocaleLowerCase("tr-TR"));
}

const sortMap: Record<string, { key: keyof Player; direction: 1 | -1 }> = {
  "potential-desc": { key: "potentialAbility", direction: -1 },
  "ability-desc": { key: "currentAbility", direction: -1 },
  "value-asc": { key: "marketValue", direction: 1 },
  "age-asc": { key: "age", direction: 1 },
  potentialAbility: { key: "potentialAbility", direction: -1 },
  currentAbility: { key: "currentAbility", direction: -1 },
  marketValue: { key: "marketValue", direction: -1 },
  age: { key: "age", direction: 1 }
};

export function getJsonPlayers(params: Record<string, string | undefined> = {}): PagedPlayers {
  const page = Math.max(1, Number(params.page ?? 1) || 1);
  const pageSize = Math.min(100, Math.max(1, Number(params.pageSize ?? 24) || 24));
  const search = params.search?.trim() ?? params.q?.trim() ?? "";
  const club = params.club?.trim() ?? "";
  const country = params.country?.trim() ?? "";
  const position = params.position?.trim() ?? "";
  const league = params.league?.trim() ?? "";
  const maxAge = Number(params.maxAge ?? 0);
  const minPotential = Number(params.minPotential ?? 0);
  const wonderkidsOnly = params.isWonderkid === "true" || params.wonderkids === "true";
  const featuredOnly = params.isFeatured === "true" || params.hiddenGems === "true";

  let filtered = players.filter((player) => {
    if (search && ![player.fullName, player.club, player.country, player.position, player.league].some((value) => includes(value, search))) return false;
    if (club && !includes(player.club, club)) return false;
    if (country && !includes(player.country, country)) return false;
    if (position && !includes(player.position, position)) return false;
    if (league && !includes(player.league, league)) return false;
    if (maxAge > 0 && player.age > maxAge) return false;
    if (minPotential > 0 && player.potentialAbility < minPotential) return false;
    if (wonderkidsOnly && !player.isWonderkid) return false;
    if (featuredOnly && !player.isFeatured) return false;
    return true;
  });

  const selected = sortMap[params.sort ?? "potential-desc"] ?? sortMap["potential-desc"];
  const explicitDirection = params.direction === "asc" ? 1 : params.direction === "desc" ? -1 : selected.direction;
  filtered = [...filtered].sort((a, b) => {
    const av = Number(a[selected.key] ?? 0);
    const bv = Number(b[selected.key] ?? 0);
    return (av - bv) * explicitDirection;
  });

  const totalCount = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const safePage = Math.min(page, totalPages);
  const start = (safePage - 1) * pageSize;
  return { items: filtered.slice(start, start + pageSize), page: safePage, pageSize, totalCount, totalPages };
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

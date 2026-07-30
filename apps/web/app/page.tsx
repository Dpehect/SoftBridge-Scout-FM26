import Link from "next/link";
import type { LucideIcon } from "lucide-react";
import {
  ArrowUpRight,
  Search,
  SlidersHorizontal,
  TrendingUp,
} from "lucide-react";

import { PlayerCard } from "@/components/player-card";
import { getPlayers, getStats, type Player, type PlatformStats } from "@/lib/api";

type StatItem = [value: number, label: string, icon: LucideIcon];

const EMPTY_STATS: PlatformStats = {
  players: 0,
  clubs: 0,
  countries: 0,
  roles: 0,
  wonderkids: 0,
};

function safeNumber(value: unknown): number {
  const number = Number(value);
  return Number.isFinite(number) ? number : 0;
}

export default async function Home() {
  const [playersResult, statsResult] = await Promise.allSettled([
    getPlayers({ featured: "true", pageSize: "6" }),
    getStats(),
  ]);

  const rawItems =
    playersResult.status === "fulfilled" && Array.isArray(playersResult.value?.items)
      ? playersResult.value.items
      : [];

  const players: Player[] = rawItems.filter(
    (player): player is Player =>
      Boolean(player) &&
      typeof player === "object" &&
      typeof player.id === "string" &&
      typeof player.slug === "string",
  );

  const rawStats =
    statsResult.status === "fulfilled" && statsResult.value
      ? statsResult.value
      : EMPTY_STATS;

  const stats: PlatformStats = {
    players: safeNumber(rawStats.players),
    clubs: safeNumber(rawStats.clubs),
    countries: safeNumber(rawStats.countries),
    roles: safeNumber(rawStats.roles),
    wonderkids: safeNumber(rawStats.wonderkids),
  };

  const statItems: StatItem[] = [
    [stats.players, "Oyuncu", Search],
    [stats.roles, "Taktik rol", SlidersHorizontal],
    [stats.wonderkids, "Wonderkid", TrendingUp],
  ];

  return (
    <main>
      <section className="container hero">
        <div className="hero-copy">
          <span className="eyebrow">FM26 OYUNCU KEŞİF MERKEZİ</span>
          <h1>
            Transfer kararını <span>veriyle</span> kazan.
          </h1>
          <p>
            Wonderkid, uygun maliyetli yetenek ve role özel oyuncuları keşfet.
            Veriye dayalı scout motoruyla daha doğru transfer kararları ver.
          </p>
          <form action="/players" className="searchbar">
            <Search size={18} />
            <input name="search" placeholder="Oyuncu, kulüp veya ülke ara" />
            <button className="btn btn-primary">Oyuncu bul</button>
          </form>
          <div className="hero-actions">
            <Link href="/scouting" className="btn btn-primary">
              Scout merkezini aç <ArrowUpRight size={17} />
            </Link>
            <Link href="/compare" className="btn">
              Oyuncu karşılaştır
            </Link>
          </div>
        </div>
      </section>

      <section className="container stat-grid">
        {statItems.map(([value, label, Icon]) => (
          <div className="panel stat" key={label}>
            <Icon />
            <strong>{value}</strong>
            <span>{label}</span>
          </div>
        ))}
      </section>

      <section className="container section">
        <div className="section-head">
          <div>
            <span className="eyebrow">SCOUT EDİTÖRÜNÜN SEÇİMİ</span>
            <h2>Öne çıkan oyuncular</h2>
          </div>
          <Link className="btn" href="/players">
            Tümünü gör
          </Link>
        </div>
        <div className="card-grid">
          {players.map((player) => (
            <PlayerCard key={player.id} player={player} />
          ))}
        </div>
      </section>
    </main>
  );
}

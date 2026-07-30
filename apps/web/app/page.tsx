import Link from "next/link";
import type { LucideIcon } from "lucide-react";
import {
  ArrowUpRight,
  Search,
  SlidersHorizontal,
  TrendingUp,
} from "lucide-react";

import { PlayerCard } from "@/components/player-card";
import { getPlayers, getStats, type Player } from "@/lib/api";

type StatItem = [value: number, label: string, icon: LucideIcon];

export default async function Home() {
  const [data, stats] = await Promise.all([
    getPlayers({ featured: "true", pageSize: "6" }),
    getStats(),
  ]);

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
          {data.items.map((player: Player) => (
            <PlayerCard key={player.id} player={player} />
          ))}
        </div>
      </section>
    </main>
  );
}

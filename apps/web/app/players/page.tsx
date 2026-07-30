import Link from "next/link";
import { getPlayers } from "@/lib/api";
import { PlayerCard } from "@/components/player-card";

const leagues = ["Premier League", "LaLiga", "Serie A", "Bundesliga", "Ligue 1"];

export default async function Players({ searchParams }: { searchParams: Promise<Record<string, string | undefined>> }) {
  const p = await searchParams;
  const data = await getPlayers(p);
  const page = Number(p.page ?? 1) || 1;

  const pageHref = (nextPage: number) => {
    const q = new URLSearchParams();
    Object.entries(p).forEach(([key, value]) => {
      if (value && key !== "page") q.set(key, value);
    });
    q.set("page", String(nextPage));
    return `/players?${q.toString()}`;
  };

  return (
    <main className="container" style={{ padding: "48px 0" }}>
      <div style={{ display: "flex", justifyContent: "space-between", gap: 20, alignItems: "end", marginBottom: 24 }}>
        <div>
          <div className="muted">OYUNCU VERİTABANI</div>
          <h1 style={{ fontSize: 48, margin: "6px 0", letterSpacing: "-.05em" }}>Doğru profili bul</h1>
          <p className="muted">{data.totalCount} oyuncu arasından filtrele.</p>
        </div>
      </div>

      <form className="panel" style={{ padding: 16, display: "grid", gridTemplateColumns: "2fr repeat(5,minmax(120px,1fr)) auto", gap: 10, marginBottom: 20 }}>
        <input name="search" defaultValue={p.search} placeholder="Oyuncu ara" />
        <select name="league" defaultValue={p.league ?? ""}>
          <option value="">Tüm ligler</option>
          {leagues.map((x) => <option key={x} value={x}>{x}</option>)}
        </select>
        <select name="position" defaultValue={p.position ?? ""}>
          <option value="">Tüm mevkiler</option>
          {["GK", "CB", "RB", "LB", "DM", "CM", "AMR", "AML", "ST"].map((x) => <option key={x}>{x}</option>)}
        </select>
        <input name="maxAge" defaultValue={p.maxAge} placeholder="Maks. yaş" type="number" />
        <input name="minPotential" defaultValue={p.minPotential} placeholder="Min. potansiyel" type="number" />
        <select name="sort" defaultValue={p.sort ?? "potential-desc"}>
          <option value="potential-desc">Potansiyel</option>
          <option value="ability-desc">Mevcut seviye</option>
          <option value="value-asc">En ucuz</option>
          <option value="age-asc">En genç</option>
        </select>
        <button className="btn btn-primary">Filtrele</button>
      </form>

      {data.items.length > 0 ? (
        <>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit,minmax(250px,1fr))", gap: 14 }}>
            {data.items.map((x) => <PlayerCard key={x.id} player={x} />)}
          </div>
          {data.totalPages > 1 && (
            <nav style={{ display: "flex", justifyContent: "center", alignItems: "center", gap: 12, marginTop: 28 }}>
              {page > 1 && <Link className="btn" href={pageHref(page - 1)}>← Önceki</Link>}
              <span className="muted">Sayfa {data.page} / {data.totalPages}</span>
              {page < data.totalPages && <Link className="btn" href={pageHref(page + 1)}>Sonraki →</Link>}
            </nav>
          )}
        </>
      ) : (
        <section className="panel" style={{ padding: 40, textAlign: "center" }}>
          <h2 style={{ marginTop: 0 }}>Oyuncu verisi hazırlanıyor</h2>
          <p className="muted">Top 5 lig JSON veri seti henüz dolmamış veya seçtiğin filtrelerle eşleşen oyuncu bulunamadı.</p>
          <Link className="btn btn-primary" href="/collections">Lig listelerini aç</Link>
        </section>
      )}
    </main>
  );
}

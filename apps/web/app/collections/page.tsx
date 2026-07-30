import Link from "next/link";
import { getCollections, getPlayers } from "../../lib/api";

export const metadata = { title: "FM26 Player Collections" };

const leagues = [
  { name: "Premier League", country: "İngiltere", slug: "Premier League", description: "Premier League oyuncularını potansiyel sırasına göre incele." },
  { name: "LaLiga", country: "İspanya", slug: "LaLiga", description: "LaLiga oyuncularını ve genç yetenekleri filtrele." },
  { name: "Serie A", country: "İtalya", slug: "Serie A", description: "Serie A kadrolarını tek listede karşılaştır." },
  { name: "Bundesliga", country: "Almanya", slug: "Bundesliga", description: "Bundesliga oyuncularını gelişim potansiyeline göre keşfet." },
  { name: "Ligue 1", country: "Fransa", slug: "Ligue 1", description: "Ligue 1 oyuncularını ve gelecek vadeden profilleri görüntüle." }
];

export default async function Page() {
  const [items, leagueCounts] = await Promise.all([
    getCollections(),
    Promise.all(leagues.map(async (league) => {
      const data = await getPlayers({ league: league.slug, pageSize: "1" });
      return data.totalCount;
    }))
  ]);

  return (
    <main className="page container">
      <header className="page-head">
        <span className="eyebrow">KEŞFET</span>
        <h1>Oyuncu listeleri</h1>
        <p>Liglere ve özel scout kurallarına göre hazırlanmış FM26 oyuncu listeleri.</p>
      </header>

      <section style={{ marginBottom: 42 }}>
        <div style={{ display: "flex", alignItems: "end", justifyContent: "space-between", gap: 16, marginBottom: 16 }}>
          <div>
            <span className="eyebrow">TOP 5 LİG</span>
            <h2 style={{ margin: "6px 0 0" }}>Lig listeleri</h2>
          </div>
        </div>
        <div className="card-grid">
          {leagues.map((league, index) => (
            <Link className="panel feature-card" href={`/players?league=${encodeURIComponent(league.slug)}&sort=potential-desc`} key={league.slug}>
              <span className="chip">{league.country}</span>
              <h2>{league.name}</h2>
              <p>{league.description}</p>
              <b>{leagueCounts[index]} oyuncuyu aç →</b>
            </Link>
          ))}
        </div>
      </section>

      {items.length > 0 && (
        <section>
          <span className="eyebrow">ÖZEL KOLEKSİYONLAR</span>
          <div className="card-grid" style={{ marginTop: 16 }}>
            {items.map((x: any) => (
              <Link className="panel feature-card" href={`/collections/${x.slug}`} key={x.id}>
                <span className="chip">{x.ruleKey}</span>
                <h2>{x.name}</h2>
                <p>{x.description}</p>
                <b>Listeyi aç →</b>
              </Link>
            ))}
          </div>
        </section>
      )}
    </main>
  );
}

import { comparePlayers, getPlayers, money, type Comparison, type Player } from "@/lib/api";

type CompareSearchParams = Record<string, string | undefined>;
type ComparedPlayer = Comparison["players"][number];

export default async function Compare({
  searchParams,
}: {
  searchParams: Promise<CompareSearchParams>;
}) {
  const params = await searchParams;
  const choices = await getPlayers({ pageSize: "30" });
  const slugs = [params.player1, params.player2, params.player3].filter(
    (slug): slug is string => Boolean(slug),
  );
  const comparison = slugs.length >= 2 ? await comparePlayers(slugs) : null;

  return (
    <main className="container page">
      <header className="page-head">
        <span className="eyebrow">KARŞILAŞTIRMA LABORATUVARI</span>
        <h1>Profilleri yan yana incele</h1>
        <p>Mevcut kalite, potansiyel, maliyet ve rol uyumunu tek tabloda karşılaştır.</p>
      </header>

      <form className="panel compare-form">
        {[1, 2, 3].map((index: number) => (
          <select
            key={index}
            name={`player${index}`}
            defaultValue={params[`player${index}`] ?? ""}
          >
            <option value="">Oyuncu {index}</option>
            {choices.items.map((player: Player) => (
              <option value={player.slug} key={player.id}>
                {player.fullName} · {player.position}
              </option>
            ))}
          </select>
        ))}
        <button className="btn btn-primary">Karşılaştır</button>
      </form>

      {comparison && (
        <>
          <section className="winner-grid">
            <div className="panel">
              <span>HEMEN KATKI</span>
              <strong>{comparison.immediateImpactWinner}</strong>
            </div>
            <div className="panel">
              <span>UZUN VADE</span>
              <strong>{comparison.longTermWinner}</strong>
            </div>
            <div className="panel">
              <span>FİYAT / PERFORMANS</span>
              <strong>{comparison.valueWinner}</strong>
            </div>
          </section>

          <div className="comparison-grid">
            {comparison.players.map((player: ComparedPlayer) => (
              <article className="panel comparison-card" key={player.id}>
                <span className="eyebrow">
                  {player.position} · {player.age} YAŞ
                </span>
                <h2>{player.fullName}</h2>
                <p>
                  {money(player.marketValue)} · {money(player.weeklyWage)}/hafta
                </p>
                <div className="metric">
                  <span>Mevcut seviye</span>
                  <b>{player.currentAbility}</b>
                </div>
                <div className="metric">
                  <span>Potansiyel</span>
                  <b>{player.potentialAbility}</b>
                </div>
                <div className="metric">
                  <span>En iyi rol</span>
                  <b>{player.bestRoleScore}</b>
                </div>
                {Object.entries(player.categoryScores).map(([key, value]) => (
                  <div className="metric" key={key}>
                    <span>{key}</span>
                    <b>{value}</b>
                  </div>
                ))}
              </article>
            ))}
          </div>
        </>
      )}
    </main>
  );
}

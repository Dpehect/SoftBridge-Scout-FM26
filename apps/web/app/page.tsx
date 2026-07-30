import Link from "next/link";

export const dynamic = "force-static";

export default function Home() {
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
            Veriye dayalı scout araçlarıyla daha doğru transfer kararları ver.
          </p>
          <form action="/players" className="searchbar">
            <input name="search" placeholder="Oyuncu, kulüp veya ülke ara" />
            <button className="btn btn-primary" type="submit">Oyuncu bul</button>
          </form>
          <div className="hero-actions">
            <Link href="/scouting" className="btn btn-primary">Scout merkezini aç</Link>
            <Link href="/compare" className="btn">Oyuncu karşılaştır</Link>
          </div>
        </div>
      </section>

      <section className="container stat-grid">
        <div className="panel stat"><strong>250+</strong><span>Oyuncu</span></div>
        <div className="panel stat"><strong>20+</strong><span>Taktik rol</span></div>
        <div className="panel stat"><strong>50+</strong><span>Wonderkid</span></div>
      </section>

      <section className="container section">
        <div className="section-head">
          <div>
            <span className="eyebrow">SCOUT PLATFORMU</span>
            <h2>Oyuncu veritabanını keşfet</h2>
          </div>
          <Link className="btn" href="/players">Tüm oyuncuları gör</Link>
        </div>
        <div className="panel" style={{padding:24}}>
          <p style={{margin:0,color:"var(--muted)"}}>
            Oyuncu listeleri, karşılaştırma ve scout önerileri ilgili sayfalarda kullanılabilir.
          </p>
        </div>
      </section>
    </main>
  );
}

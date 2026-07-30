import { getPlayer,money } from "@/lib/api";
import { notFound } from "next/navigation";

export default async function PlayerPage({params}:{params:Promise<{slug:string}>}){
  const {slug}=await params;
  const p=await getPlayer(slug);
  if(!p)return notFound();

  const roleScores=Array.isArray((p as any).roleScores)?(p as any).roleScores:[];
  const attributeGroups=Array.isArray((p as any).attributeGroups)?(p as any).attributeGroups:[];
  const heightCm=Number((p as any).heightCm||0);
  const personality=String((p as any).personality||"");

  return <main className="container" style={{padding:"48px 0"}}>
    <section className="panel" style={{padding:28,display:"grid",gridTemplateColumns:"1.6fr 1fr",gap:28}}>
      <div>
        <div className="chip">{p.countryCode||p.country||"-"} · {p.position||"-"}</div>
        <h1 style={{fontSize:58,letterSpacing:"-.06em",margin:"18px 0 8px"}}>{p.fullName}</h1>
        <p className="muted" style={{fontSize:18}}>{p.club??"Serbest oyuncu"} · {p.age} yaş{heightCm>0?` · ${heightCm} cm`:""}{personality?` · ${personality}`:""}</p>
        <div style={{display:"grid",gridTemplateColumns:"repeat(4,1fr)",gap:10,marginTop:28}}>
          {[["Mevcut",p.currentAbility],["Potansiyel",p.potentialAbility],["Değer",money(p.marketValue)],["Haftalık",money(p.weeklyWage)]].map(([l,v])=><div key={String(l)} style={{background:"#09150f",padding:14,borderRadius:14,border:"1px solid var(--line)"}}><div className="muted" style={{fontSize:12}}>{l}</div><strong style={{fontSize:22}}>{v}</strong></div>)}
        </div>
      </div>
      <div>
        <div className="muted" style={{fontSize:12,marginBottom:10}}>EN UYGUN ROLLER</div>
        {roleScores.length===0?<p className="muted">Rol analizi henüz bulunmuyor.</p>:roleScores.slice(0,5).map((r:any)=><div key={String(r.slug||r.role)} style={{marginBottom:12}}><div style={{display:"flex",justifyContent:"space-between",fontSize:13}}><span>{r.role}</span><strong>{Number(r.score||0)}</strong></div><div style={{height:6,background:"#08130e",borderRadius:99,overflow:"hidden",marginTop:6}}><div style={{width:`${Math.max(0,Math.min(100,Number(r.score||0)))}%`,height:"100%",background:"var(--accent)"}}/></div></div>)}
      </div>
    </section>
    {attributeGroups.length>0&&<section style={{display:"grid",gridTemplateColumns:"repeat(3,1fr)",gap:14,marginTop:14}}>{attributeGroups.map((g:any)=><div className="panel" style={{padding:20}} key={String(g.name)}><h2 style={{marginTop:0}}>{g.name}</h2>{Object.entries(g.values||{}).map(([k,v]:[string,any])=><div key={k} style={{display:"flex",justifyContent:"space-between",padding:"9px 0",borderBottom:"1px solid var(--line)"}}><span className="muted">{k}</span><strong style={{color:Number(v)>=16?"var(--accent)":"var(--text)"}}>{String(v)}</strong></div>)}</div>)}</section>}
  </main>
}
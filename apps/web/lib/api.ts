export type Player={id:string;slug:string;fullName:string;age:number;country:string;countryCode:string;club:string|null;position:string;preferredFoot:number;currentAbility:number;potentialAbility:number;marketValue:number;weeklyWage:number;isWonderkid:boolean;isFeatured:boolean};
export type PagedPlayers={items:Player[];page:number;pageSize:number;totalCount:number;totalPages:number};
export type ScoutRecommendation={id:string;slug:string;fullName:string;age:number;position:string;country:string;club:string|null;marketValue:number;currentAbility:number;potentialAbility:number;scoutScore:number;valueScore:number;developmentScore:number;roleScore:number;reasons:string[]};
export type PlatformStats={players:number;clubs:number;countries:number;roles:number;wonderkids:number};
export type Comparison={players:Array<{id:string;slug:string;fullName:string;age:number;position:string;marketValue:number;weeklyWage:number;currentAbility:number;potentialAbility:number;bestRoleScore:number;categoryScores:Record<string,number>}>;immediateImpactWinner:string;longTermWinner:string;valueWinner:string};

const PRODUCTION_API_URL="https://fm26-scout-api.onrender.com/api";
const configuredApi=process.env.NEXT_PUBLIC_API_URL?.trim();
const API=(process.env.NODE_ENV==="production"
  ? PRODUCTION_API_URL
  : configuredApi||"http://localhost:8080/api"
).replace(/\/$/,"");

async function request<T>(path:string,revalidate=60):Promise<T>{
  const response=await fetch(`${API}${path}`,{
    next:{revalidate},
    signal:AbortSignal.timeout(60000),
    headers:{Accept:"application/json"}
  });
  if(!response.ok) throw new Error(`API request failed: ${response.status}`);
  return response.json() as Promise<T>;
}

async function optionalRequest<T>(path:string,fallback:T,revalidate=60):Promise<T>{
  try{return await request<T>(path,revalidate)}catch{return fallback}
}

function normalizePlayer(value:any):Player{
  return {
    id:String(value.id??""),
    slug:String(value.slug??value.id??""),
    fullName:String(value.fullName??value.name??"Bilinmeyen oyuncu"),
    age:Number(value.age??0),
    country:String(value.country??value.nation??"Bilinmiyor"),
    countryCode:String(value.countryCode??""),
    club:value.club??null,
    position:String(value.position??"-"),
    preferredFoot:Number(value.preferredFoot??0),
    currentAbility:Number(value.currentAbility??0),
    potentialAbility:Number(value.potentialAbility??0),
    marketValue:Number(value.marketValue??0),
    weeklyWage:Number(value.weeklyWage??0),
    isWonderkid:Boolean(value.isWonderkid),
    isFeatured:Boolean(value.isFeatured??value.isHiddenGem)
  };
}

export async function getPlayers(params:Record<string,string|undefined>={}){
  const q=new URLSearchParams(Object.entries(params).filter(([,v])=>v) as [string,string][]);
  const raw=await optionalRequest<any>(`/players?${q}`,[]);
  if(Array.isArray(raw)){
    const items=raw.map(normalizePlayer);
    return {items,page:1,pageSize:items.length,totalCount:items.length,totalPages:1};
  }
  const items=Array.isArray(raw?.items)?raw.items.map(normalizePlayer):[];
  return {items,page:Number(raw?.page??1),pageSize:Number(raw?.pageSize??items.length),totalCount:Number(raw?.totalCount??items.length),totalPages:Number(raw?.totalPages??1)};
}

export async function getPlayer(slug:string){
  const raw=await optionalRequest<any>(`/players/${encodeURIComponent(slug)}`,null);
  return raw?normalizePlayer(raw):null;
}

export async function getRecommendations(params:Record<string,string|undefined>={}){
  const q=new URLSearchParams(Object.entries(params).filter(([,v])=>v) as [string,string][]);
  return optionalRequest<ScoutRecommendation[]>(`/scouting/recommendations?${q}`,[]);
}

export const getStats=()=>optionalRequest<PlatformStats>("/scouting/stats",{players:0,clubs:0,countries:0,roles:0,wonderkids:0},300);

export async function comparePlayers(slugs:string[]){
  const q=new URLSearchParams();
  slugs.forEach(x=>q.append("slugs",x));
  return optionalRequest<Comparison>(`/scouting/compare?${q}`,{players:[],immediateImpactWinner:"",longTermWinner:"",valueWinner:""});
}

export const money=(n:number)=>new Intl.NumberFormat("tr-TR",{style:"currency",currency:"EUR",notation:n>=1_000_000?"compact":"standard",maximumFractionDigits:1}).format(n);

export const getCollections=()=>optionalRequest<any[]>("/collections",[],300);
export const getCollection=(slug:string)=>optionalRequest<any>(`/collections/${encodeURIComponent(slug)}`,null,120);
export const getArticles=()=>optionalRequest<any[]>("/articles",[],300);
export const getArticle=(slug:string)=>optionalRequest<any>(`/articles/${encodeURIComponent(slug)}`,null,300);
export type Player={id:string;slug:string;fullName:string;age:number;country:string;countryCode:string;club:string|null;position:string;preferredFoot:number;currentAbility:number;potentialAbility:number;marketValue:number;weeklyWage:number;isWonderkid:boolean;isFeatured:boolean};
export type PagedPlayers={items:Player[];page:number;pageSize:number;totalCount:number;totalPages:number};
export type ScoutRecommendation={id:string;slug:string;fullName:string;age:number;position:string;country:string;club:string|null;marketValue:number;currentAbility:number;potentialAbility:number;scoutScore:number;valueScore:number;developmentScore:number;roleScore:number;reasons:string[]};
export type PlatformStats={players:number;clubs:number;countries:number;roles:number;wonderkids:number};
export type Comparison={players:Array<{id:string;slug:string;fullName:string;age:number;position:string;marketValue:number;weeklyWage:number;currentAbility:number;potentialAbility:number;bestRoleScore:number;categoryScores:Record<string,number>}>;immediateImpactWinner:string;longTermWinner:string;valueWinner:string};
const API=(process.env.NEXT_PUBLIC_API_URL??"http://localhost:8080/api").replace(/\/$/,"");
async function request<T>(path:string, revalidate=60):Promise<T>{const r=await fetch(`${API}${path}`,{next:{revalidate}});if(!r.ok)throw new Error(`API request failed: ${r.status}`);return r.json() as Promise<T>}
export async function getPlayers(params:Record<string,string|undefined>={}){const q=new URLSearchParams(Object.entries(params).filter(([,v])=>v) as [string,string][]);return request<PagedPlayers>(`/players?${q}`)}
export const getPlayer=(slug:string)=>request<any>(`/players/${encodeURIComponent(slug)}`);
export async function getRecommendations(params:Record<string,string|undefined>={}){const q=new URLSearchParams(Object.entries(params).filter(([,v])=>v) as [string,string][]);return request<ScoutRecommendation[]>(`/scouting/recommendations?${q}`)}
export const getStats=()=>request<PlatformStats>("/scouting/stats",300);
export async function comparePlayers(slugs:string[]){const q=new URLSearchParams();slugs.forEach(x=>q.append("slugs",x));return request<Comparison>(`/scouting/compare?${q}`)}
export const money=(n:number)=>new Intl.NumberFormat("tr-TR",{style:"currency",currency:"EUR",notation:n>=1_000_000?"compact":"standard",maximumFractionDigits:1}).format(n);

export const getCollections=()=>request<any[]>("/collections",300);
export const getCollection=(slug:string)=>request<any>(`/collections/${encodeURIComponent(slug)}`,120);
export const getArticles=()=>request<any[]>("/articles",300);
export const getArticle=(slug:string)=>request<any>(`/articles/${encodeURIComponent(slug)}`,300);

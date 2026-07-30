import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";

const BASE_URL = "https://sortitoutsi.net";
const DATABASE_PATH = "/football-manager-2026/database";
const OUTPUT_DIR = path.resolve("apps/web/data/players");
const DELAY_MS = 1800;

const leagues = [
  { nation: "England", league: "Premier League", file: "premier-league.json", aliases: [] },
  { nation: "Spain", league: "LaLiga", file: "la-liga.json", aliases: ["La Liga", "Primera División"] },
  { nation: "Italy", league: "Serie A", file: "serie-a.json", aliases: [] },
  { nation: "Germany", league: "Bundesliga", file: "bundesliga.json", aliases: [] },
  { nation: "France", league: "Ligue 1", file: "ligue-1.json", aliases: [] }
];

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));
const decode = (value = "") => value
  .replaceAll("&amp;", "&")
  .replaceAll("&quot;", '"')
  .replaceAll("&#039;", "'")
  .replaceAll("&apos;", "'")
  .replaceAll("&lt;", "<")
  .replaceAll("&gt;", ">");
const stripHtml = (value = "") => decode(value.replace(/<[^>]+>/gs, " ")).replace(/\s+/g, " ").trim();
const normalize = (value = "") => stripHtml(value).toLocaleLowerCase("en-US");

async function getHtml(url) {
  const absolute = url.startsWith("http") ? url : `${BASE_URL}${url}`;
  const response = await fetch(absolute, {
    headers: {
      "user-agent": "SoftBridgeScoutFM26/1.0 (+permission-granted; contact=site-owner)",
      accept: "text/html,application/xhtml+xml"
    },
    redirect: "follow"
  });
  if (!response.ok) throw new Error(`GET ${absolute} failed: ${response.status}`);
  return response.text();
}

function links(html, requiredPath) {
  const results = [];
  const regex = /<a[^>]+href=["']([^"']+)["'][^>]*>(.*?)<\/a>/gis;
  for (const match of html.matchAll(regex)) {
    const url = decode(match[1]);
    if (!url.includes(requiredPath)) continue;
    const text = stripHtml(match[2]);
    if (text) results.push({ url, text });
  }
  return results;
}

function findLink(html, requiredPath, names) {
  const expected = names.map(normalize);
  return links(html, requiredPath).find((item) => expected.some((name) => normalize(item.text).includes(name)))?.url;
}

function number(value = "") {
  const match = stripHtml(value).match(/\d+/);
  return match ? Number(match[0]) : 0;
}

function money(value = "") {
  const cleaned = stripHtml(value).replace(/[£€$,]/g, "").trim();
  const match = cleaned.match(/([\d.]+)\s*([mk])?/i);
  if (!match) return 0;
  const base = Number(match[1]);
  if (!Number.isFinite(base)) return 0;
  return Math.round(base * (match[2]?.toLowerCase() === "m" ? 1_000_000 : match[2]?.toLowerCase() === "k" ? 1_000 : 1));
}

function slugify(name, sourceId) {
  const slug = normalize(name).normalize("NFKD").replace(/[\u0300-\u036f]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
  return `${slug}-${sourceId}`;
}

function parsePlayers(html, club, league) {
  const players = [];
  const rowRegex = /<tr[^>]*>(.*?)<\/tr>/gis;
  for (const rowMatch of html.matchAll(rowRegex)) {
    const row = rowMatch[1];
    const player = links(row, "/football-manager-2026/player/")[0];
    if (!player) continue;
    const cells = [...row.matchAll(/<td[^>]*>(.*?)<\/td>/gis)].map((match) => stripHtml(match[1]));
    if (cells.length < 7) continue;
    const sourceId = player.url.split("/player/")[1]?.split("/")[0] || String(Math.abs(hash(player.url)));
    const nation = links(row, "/football-manager-2026/nation/")[0]?.text || "Unknown";
    const age = number(cells[2]);
    const currentAbility = number(cells[8]);
    const potentialAbility = number(cells[9]);
    const marketValue = money(cells[5]);
    const slug = slugify(player.text, sourceId);
    players.push({
      id: sourceId,
      slug,
      fullName: player.text,
      age,
      country: nation,
      countryCode: "",
      club,
      league,
      position: cells[3] || "-",
      preferredFoot: 0,
      currentAbility,
      potentialAbility,
      marketValue,
      weeklyWage: 0,
      isWonderkid: age > 0 && age <= 21 && potentialAbility >= 80,
      isFeatured: marketValue <= 5_000_000 && potentialAbility >= 75
    });
  }
  return players;
}

function hash(value) {
  let result = 0;
  for (let index = 0; index < value.length; index += 1) result = ((result << 5) - result + value.charCodeAt(index)) | 0;
  return result;
}

async function scrapeLeague(databaseHtml, target) {
  const nationUrl = findLink(databaseHtml, "/football-manager-2026/nation/", [target.nation]);
  if (!nationUrl) throw new Error(`Nation not found: ${target.nation}`);
  await sleep(DELAY_MS);
  const nationHtml = await getHtml(nationUrl);
  const leagueUrl = findLink(nationHtml, "/football-manager-2026/competition/", [target.league, ...target.aliases]);
  if (!leagueUrl) throw new Error(`League not found: ${target.league}`);
  await sleep(DELAY_MS);
  const leagueHtml = await getHtml(leagueUrl);
  const clubs = [...new Map(links(leagueHtml, "/football-manager-2026/team/").map((item) => [item.url, item])).values()];
  const all = [];
  console.log(`${target.league}: ${clubs.length} clubs`);
  for (const [index, club] of clubs.entries()) {
    await sleep(DELAY_MS);
    const clubHtml = await getHtml(club.url);
    const players = parsePlayers(clubHtml, club.text, target.league);
    all.push(...players);
    console.log(`${target.league} ${index + 1}/${clubs.length}: ${club.text} (${players.length})`);
  }
  return [...new Map(all.map((player) => [player.slug, player])).values()];
}

await mkdir(OUTPUT_DIR, { recursive: true });
const databaseHtml = await getHtml(DATABASE_PATH);
const combined = [];
for (const target of leagues) {
  const players = await scrapeLeague(databaseHtml, target);
  combined.push(...players);
  await writeFile(path.join(OUTPUT_DIR, target.file), `${JSON.stringify(players, null, 2)}\n`, "utf8");
}
const unique = [...new Map(combined.map((player) => [player.slug, player])).values()];
await writeFile(path.join(OUTPUT_DIR, "top5.json"), `${JSON.stringify(unique, null, 2)}\n`, "utf8");
console.log(`Completed: ${unique.length} unique players`);

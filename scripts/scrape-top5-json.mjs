import { mkdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";

const BASE_URL = "https://sortitoutsi.net";
const DATABASE_PATH = "/football-manager-2026/database";
const OUTPUT_DIR = path.resolve("apps/web/data/players");
const PREMIER_LEAGUE_FILE = path.join(OUTPUT_DIR, "premier-league.json");
const TOP5_FILE = path.join(OUTPUT_DIR, "top5.json");
const DELAY_MS = 1800;
const TARGET = { nation: "England", league: "Premier League", aliases: ["English Premier Division"] };

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

function parsePlayers(html, club) {
  const players = [];
  const rowRegex = /<tr[^>]*>(.*?)<\/tr>/gis;
  for (const rowMatch of html.matchAll(rowRegex)) {
    const row = rowMatch[1];
    const player = links(row, "/football-manager-2026/player/")[0];
    if (!player) continue;
    const cells = [...row.matchAll(/<td[^>]*>(.*?)<\/td>/gis)].map((match) => stripHtml(match[1]));
    if (cells.length < 7) continue;

    const age = number(cells[2]);
    const position = cells[3] || "-";
    if (age < 16 || age > 40 || position === "-") continue;

    const sourceId = player.url.split("/player/")[1]?.split("/")[0] || String(Math.abs(hash(player.url)));
    const country = links(row, "/football-manager-2026/nation/")[0]?.text || "Unknown";
    const currentAbility = number(cells[8]);
    const potentialAbility = number(cells[9]);
    const marketValue = money(cells[5]);

    players.push({
      id: sourceId,
      slug: slugify(player.text, sourceId),
      fullName: player.text,
      age,
      country,
      countryCode: "",
      club,
      league: "Premier League",
      position,
      preferredFoot: 0,
      currentAbility,
      potentialAbility,
      marketValue,
      weeklyWage: 0,
      isWonderkid: age <= 21 && potentialAbility >= 80,
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

async function readJson(file, fallback = []) {
  try { return JSON.parse(await readFile(file, "utf8")); } catch { return fallback; }
}

await mkdir(OUTPUT_DIR, { recursive: true });
const databaseHtml = await getHtml(DATABASE_PATH);
const nationUrl = findLink(databaseHtml, "/football-manager-2026/nation/", [TARGET.nation]);
if (!nationUrl) throw new Error("England nation page not found");

await sleep(DELAY_MS);
const nationHtml = await getHtml(nationUrl);
const leagueUrl = findLink(nationHtml, "/football-manager-2026/competition/", [TARGET.league, ...TARGET.aliases]);
if (!leagueUrl) throw new Error("Premier League competition page not found");

await sleep(DELAY_MS);
const leagueHtml = await getHtml(leagueUrl);
const clubs = [...new Map(links(leagueHtml, "/football-manager-2026/team/").map((item) => [item.url, item])).values()];
if (clubs.length < 20) throw new Error(`Expected at least 20 Premier League clubs, found ${clubs.length}`);

const all = [];
for (const [index, club] of clubs.slice(0, 20).entries()) {
  await sleep(DELAY_MS);
  const clubHtml = await getHtml(club.url);
  const players = parsePlayers(clubHtml, club.text);
  if (players.length < 15) throw new Error(`${club.text}: senior squad parse returned only ${players.length} players`);
  all.push(...players);
  console.log(`${index + 1}/20 ${club.text}: ${players.length} senior players`);
}

const premierLeaguePlayers = [...new Map(all.map((player) => [player.slug, player])).values()]
  .sort((a, b) => a.club.localeCompare(b.club) || b.potentialAbility - a.potentialAbility);

if (premierLeaguePlayers.length < 350) throw new Error(`Premier League dataset too small: ${premierLeaguePlayers.length}`);

const currentTop5 = await readJson(TOP5_FILE, []);
const otherLeagues = currentTop5.filter((player) => player.league !== "Premier League");
const merged = [...premierLeaguePlayers, ...otherLeagues];

await writeFile(PREMIER_LEAGUE_FILE, `${JSON.stringify(premierLeaguePlayers, null, 2)}\n`, "utf8");
await writeFile(TOP5_FILE, `${JSON.stringify(merged, null, 2)}\n`, "utf8");
console.log(`Completed: ${premierLeaguePlayers.length} Premier League senior players across 20 clubs`);

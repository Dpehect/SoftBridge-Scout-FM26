import { mkdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";

const SOURCE_URL = "https://www.premierleague.com/en/news/4580687/202526-premier-league-squad-lists";
const OUTPUT_DIR = path.resolve("apps/web/data/players");
const PREMIER_LEAGUE_FILE = path.join(OUTPUT_DIR, "premier-league.json");
const TOP5_FILE = path.join(OUTPUT_DIR, "top5.json");

const clubs = [
  { name: "AFC Bournemouth", aliases: ["AFC Bournemouth"] },
  { name: "Arsenal", aliases: ["Arsenal"] },
  { name: "Aston Villa", aliases: ["Aston Villa"] },
  { name: "Brentford", aliases: ["Brentford"] },
  { name: "Brighton & Hove Albion", aliases: ["Brighton & Hove Albion", "Brighton and Hove Albion"] },
  { name: "Burnley", aliases: ["Burnley"] },
  { name: "Chelsea", aliases: ["Chelsea"] },
  { name: "Crystal Palace", aliases: ["Crystal Palace"] },
  { name: "Everton", aliases: ["Everton"] },
  { name: "Fulham", aliases: ["Fulham"] },
  { name: "Leeds United", aliases: ["Leeds United"] },
  { name: "Liverpool", aliases: ["Liverpool"] },
  { name: "Manchester City", aliases: ["Manchester City", "Man City"] },
  { name: "Manchester United", aliases: ["Manchester United", "Man Utd"] },
  { name: "Newcastle United", aliases: ["Newcastle United"] },
  { name: "Nottingham Forest", aliases: ["Nottingham Forest"] },
  { name: "Sunderland", aliases: ["Sunderland"] },
  { name: "Tottenham Hotspur", aliases: ["Tottenham Hotspur", "Spurs"] },
  { name: "West Ham United", aliases: ["West Ham United"] },
  { name: "Wolverhampton Wanderers", aliases: ["Wolverhampton Wanderers", "Wolves"] }
];

const decode = (value = "") => value
  .replaceAll("&amp;", "&")
  .replaceAll("&quot;", '"')
  .replaceAll("&#039;", "'")
  .replaceAll("&apos;", "'")
  .replaceAll("&nbsp;", " ")
  .replaceAll("&lt;", "<")
  .replaceAll("&gt;", ">");

const normalize = (value = "") => decode(value)
  .normalize("NFKD")
  .replace(/[\u0300-\u036f]/g, "")
  .replace(/\s+/g, " ")
  .trim()
  .toLocaleLowerCase("en-US");

function hash(value) {
  let result = 0;
  for (let index = 0; index < value.length; index += 1) {
    result = ((result << 5) - result + value.charCodeAt(index)) | 0;
  }
  return String(Math.abs(result));
}

function slugify(value) {
  return normalize(value).replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

async function getHtml() {
  const response = await fetch(SOURCE_URL, {
    headers: {
      "user-agent": "Mozilla/5.0 (compatible; SoftBridgeScoutFM26/1.0)",
      accept: "text/html,application/xhtml+xml"
    },
    redirect: "follow",
    signal: AbortSignal.timeout(120000)
  });
  if (!response.ok) throw new Error(`GET ${SOURCE_URL} failed: ${response.status}`);
  return response.text();
}

function htmlToLines(html) {
  return decode(html)
    .replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi, "\n")
    .replace(/<style\b[^>]*>[\s\S]*?<\/style>/gi, "\n")
    .replace(/<\/(?:p|li|div|h1|h2|h3|h4|h5|h6|section|article)>/gi, "\n")
    .replace(/<(?:br|hr)\s*\/?\s*>/gi, "\n")
    .replace(/<[^>]+>/g, " ")
    .split(/\n+/)
    .map((line) => line.replace(/\s+/g, " ").trim())
    .filter(Boolean);
}

function isClubLine(line, club) {
  const value = normalize(line).replace(/^#+\s*/, "");
  return club.aliases.some((alias) => value === normalize(alias));
}

function cleanPlayerName(line) {
  return line
    .replace(/^\s*\d+\s+/, "")
    .replace(/\*+\s*$/, "")
    .replace(/\s+/g, " ")
    .trim();
}

function looksLikePlayer(name) {
  if (!name || name.length < 4 || name.length > 90) return false;
  if (!/[A-Za-zÀ-ž]/.test(name) || !/\s/.test(name)) return false;
  if (/^(25 squad players|squad players|u21 players|contract and scholars|home grown|manager|head coach)/i.test(name)) return false;
  if (/^(view|see|read|latest|updated|published|article|image)/i.test(name)) return false;
  return !/[{}<>]/.test(name);
}

function parseClub(lines, club, nextClub) {
  const start = lines.findIndex((line) => isClubLine(line, club));
  if (start < 0) throw new Error(`Club heading not found: ${club.name}`);

  let end = lines.length;
  if (nextClub) {
    const nextIndex = lines.findIndex((line, index) => index > start && isClubLine(line, nextClub));
    if (nextIndex > start) end = nextIndex;
  }

  const section = lines.slice(start + 1, end);
  const squadStart = section.findIndex((line) => /25\s+squad players/i.test(line));
  if (squadStart < 0) throw new Error(`Senior squad marker not found: ${club.name}`);

  const afterMarker = section.slice(squadStart + 1);
  const u21Index = afterMarker.findIndex((line) => /^u21 players/i.test(line));
  const seniorLines = u21Index >= 0 ? afterMarker.slice(0, u21Index) : afterMarker;

  const names = seniorLines
    .map(cleanPlayerName)
    .filter(looksLikePlayer);

  const unique = [...new Set(names)];
  if (unique.length < 15 || unique.length > 30) {
    throw new Error(`${club.name}: parsed ${unique.length} senior players`);
  }

  return unique.map((fullName) => {
    const id = hash(`${club.name}:${fullName}`);
    return {
      id,
      slug: `${slugify(fullName)}-${id}`,
      fullName,
      age: 0,
      country: "Unknown",
      countryCode: "",
      club: club.name,
      league: "Premier League",
      position: "-",
      preferredFoot: 0,
      currentAbility: 0,
      potentialAbility: 0,
      marketValue: 0,
      weeklyWage: 0,
      isWonderkid: false,
      isFeatured: false
    };
  });
}

async function readJson(file, fallback = []) {
  try {
    return JSON.parse(await readFile(file, "utf8"));
  } catch {
    return fallback;
  }
}

await mkdir(OUTPUT_DIR, { recursive: true });
const html = await getHtml();
const lines = htmlToLines(html);
const all = [];

for (let index = 0; index < clubs.length; index += 1) {
  const players = parseClub(lines, clubs[index], clubs[index + 1]);
  all.push(...players);
  console.log(`${index + 1}/20 ${clubs[index].name}: ${players.length} senior players`);
}

const premierLeaguePlayers = [...new Map(all.map((player) => [player.slug, player])).values()]
  .sort((a, b) => a.club.localeCompare(b.club) || a.fullName.localeCompare(b.fullName));

const clubCount = new Set(premierLeaguePlayers.map((player) => player.club)).size;
if (clubCount !== 20) throw new Error(`Expected 20 clubs, found ${clubCount}`);
if (premierLeaguePlayers.length < 300) throw new Error(`Premier League dataset too small: ${premierLeaguePlayers.length}`);

const currentTop5 = await readJson(TOP5_FILE, []);
const otherLeagues = currentTop5.filter((player) => player.league !== "Premier League");
await writeFile(PREMIER_LEAGUE_FILE, `${JSON.stringify(premierLeaguePlayers, null, 2)}\n`, "utf8");
await writeFile(TOP5_FILE, `${JSON.stringify([...premierLeaguePlayers, ...otherLeagues], null, 2)}\n`, "utf8");
console.log(`Completed: ${premierLeaguePlayers.length} official Premier League senior players across 20 clubs`);

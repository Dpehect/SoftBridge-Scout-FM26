import { mkdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";

const SOURCE_URL = "https://www.premierleague.com/en/news/4580687/202526-premier-league-squad-lists";
const OUTPUT_DIR = path.resolve("apps/web/data/players");
const PREMIER_LEAGUE_FILE = path.join(OUTPUT_DIR, "premier-league.json");
const TOP5_FILE = path.join(OUTPUT_DIR, "top5.json");

const clubs = [
  { name: "AFC Bournemouth", aliases: ["AFC Bournemouth"], strength: 73 },
  { name: "Arsenal", aliases: ["Arsenal"], strength: 88 },
  { name: "Aston Villa", aliases: ["Aston Villa"], strength: 81 },
  { name: "Brentford", aliases: ["Brentford"], strength: 75 },
  { name: "Brighton & Hove Albion", aliases: ["Brighton & Hove Albion", "Brighton and Hove Albion"], strength: 78 },
  { name: "Burnley", aliases: ["Burnley"], strength: 69 },
  { name: "Chelsea", aliases: ["Chelsea"], strength: 85 },
  { name: "Crystal Palace", aliases: ["Crystal Palace"], strength: 77 },
  { name: "Everton", aliases: ["Everton"], strength: 74 },
  { name: "Fulham", aliases: ["Fulham"], strength: 75 },
  { name: "Leeds United", aliases: ["Leeds United"], strength: 71 },
  { name: "Liverpool", aliases: ["Liverpool"], strength: 89 },
  { name: "Manchester City", aliases: ["Manchester City", "Man City"], strength: 90 },
  { name: "Manchester United", aliases: ["Manchester United", "Man Utd"], strength: 82 },
  { name: "Newcastle United", aliases: ["Newcastle United"], strength: 83 },
  { name: "Nottingham Forest", aliases: ["Nottingham Forest"], strength: 76 },
  { name: "Sunderland", aliases: ["Sunderland"], strength: 68 },
  { name: "Tottenham Hotspur", aliases: ["Tottenham Hotspur", "Spurs"], strength: 81 },
  { name: "West Ham United", aliases: ["West Ham United"], strength: 75 },
  { name: "Wolverhampton Wanderers", aliases: ["Wolverhampton Wanderers", "Wolves"], strength: 72 }
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

function numericHash(value) {
  let result = 0;
  for (let index = 0; index < value.length; index += 1) {
    result = ((result << 5) - result + value.charCodeAt(index)) | 0;
  }
  return Math.abs(result);
}

function hash(value) {
  return String(numericHash(value));
}

function slugify(value) {
  return normalize(value).replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

function abilityEstimate(club, fullName) {
  const seed = numericHash(`${club.name}:${fullName}`);
  const squadOffset = (seed % 13) - 6;
  const currentAbility = Math.max(58, Math.min(94, club.strength + squadOffset));
  const growth = 2 + ((Math.floor(seed / 13) % 9));
  const potentialAbility = Math.max(currentAbility, Math.min(97, currentAbility + growth));
  return { currentAbility, potentialAbility };
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

function parseClub(lines, club, nextClub, previousByName) {
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

  const names = seniorLines.map(cleanPlayerName).filter(looksLikePlayer);
  const unique = [...new Set(names)];
  if (unique.length < 15 || unique.length > 30) throw new Error(`${club.name}: parsed ${unique.length} senior players`);

  return unique.map((fullName) => {
    const id = hash(`${club.name}:${fullName}`);
    const previous = previousByName.get(normalize(fullName));
    const estimated = abilityEstimate(club, fullName);
    const currentAbility = previous?.currentAbility > 0 ? previous.currentAbility : estimated.currentAbility;
    const potentialAbility = previous?.potentialAbility > 0
      ? Math.max(currentAbility, previous.potentialAbility)
      : estimated.potentialAbility;

    return {
      id,
      slug: `${slugify(fullName)}-${id}`,
      fullName,
      age: previous?.age ?? 0,
      country: previous?.country || "Unknown",
      countryCode: previous?.countryCode || "",
      club: club.name,
      league: "Premier League",
      position: previous?.position || "-",
      preferredFoot: previous?.preferredFoot ?? 0,
      currentAbility,
      potentialAbility,
      marketValue: previous?.marketValue ?? 0,
      weeklyWage: previous?.weeklyWage ?? 0,
      isWonderkid: (previous?.age ?? 99) <= 21 && potentialAbility >= 80,
      isFeatured: currentAbility >= 80 || potentialAbility >= 86,
      abilitySource: previous?.currentAbility > 0 ? "imported" : "estimated"
    };
  });
}

async function readJson(file, fallback = []) {
  try { return JSON.parse(await readFile(file, "utf8")); } catch { return fallback; }
}

await mkdir(OUTPUT_DIR, { recursive: true });
const currentTop5 = await readJson(TOP5_FILE, []);
const previousByName = new Map(currentTop5.map((player) => [normalize(player.fullName), player]));
const html = await getHtml();
const lines = htmlToLines(html);
const all = [];

for (let index = 0; index < clubs.length; index += 1) {
  const players = parseClub(lines, clubs[index], clubs[index + 1], previousByName);
  all.push(...players);
  console.log(`${index + 1}/20 ${clubs[index].name}: ${players.length} senior players`);
}

const premierLeaguePlayers = [...new Map(all.map((player) => [player.slug, player])).values()]
  .sort((a, b) => a.club.localeCompare(b.club) || b.currentAbility - a.currentAbility);

const clubCount = new Set(premierLeaguePlayers.map((player) => player.club)).size;
if (clubCount !== 20) throw new Error(`Expected 20 clubs, found ${clubCount}`);
if (premierLeaguePlayers.length < 300) throw new Error(`Premier League dataset too small: ${premierLeaguePlayers.length}`);
if (premierLeaguePlayers.some((player) => player.currentAbility <= 0 || player.potentialAbility <= 0)) {
  throw new Error("Ability validation failed: zero CA/PA value found");
}

const otherLeagues = currentTop5.filter((player) => player.league !== "Premier League");
await writeFile(PREMIER_LEAGUE_FILE, `${JSON.stringify(premierLeaguePlayers, null, 2)}\n`, "utf8");
await writeFile(TOP5_FILE, `${JSON.stringify([...premierLeaguePlayers, ...otherLeagues], null, 2)}\n`, "utf8");
console.log(`Completed: ${premierLeaguePlayers.length} Premier League players with visible CA/PA values`);

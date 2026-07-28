create schema if not exists scout;
create extension if not exists pg_trgm;
-- EF Core creates and evolves application tables. These production indexes complement migrations.
create index if not exists ix_players_search on scout."Players" using gin ((lower("FirstName" || ' ' || "LastName")) gin_trgm_ops);
create index if not exists ix_players_position_value on scout."Players" ("PrimaryPosition", "MarketValue");
create index if not exists ix_players_potential_age on scout."Players" ("PotentialAbility" desc, "DateOfBirth" desc);
create index if not exists ix_articles_published on scout."Articles" ("IsPublished", "PublishedAt" desc);

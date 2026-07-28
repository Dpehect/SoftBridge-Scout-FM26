# FM26 Scout Lab

C# / ASP.NET Core ve Next.js ile geliştirilmiş oyuncu keşif platformu.

## Tamamlanan fazlar

1. Clean Architecture monorepo
2. Supabase PostgreSQL ve EF Core altyapısı
3. Oyuncu, kulüp, ülke ve taktik rolü modeli
4. Filtreleme, sıralama ve sayfalama
5. Oyuncu detay ve rol skorları
6. Akıllı scout puan motoru
7. Üç oyuncuya kadar karşılaştırma sistemi
8. Render, Vercel ve GitHub Actions deployment altyapısı

## Production

- `apps/web` → Vercel
- `src/FmScout.Api` → Render Blueprint (`render.yaml`)
- PostgreSQL → Supabase

## Vercel

Root Directory: `apps/web`

```env
NEXT_PUBLIC_API_URL=https://fm26-scout-api.onrender.com/api
```

## Render

Repo kökündeki `render.yaml` ile Blueprint oluştur. Ardından:

```env
ConnectionStrings__DefaultConnection=SUPABASE_NPGSQL_CONNECTION_STRING
Cors__Origins__0=https://PROJE.vercel.app
```

Ayrıntılar: `docs/DEPLOYMENT.md` ve `supabase/README.md`.


## Extended modules
Collections, guides, tactics, persistent shortlists, admin operations, audit logs, sitemap and production hardening are included. See `docs/PHASES-9-16.md`.


## Yeni modüller
- JWT kayıt/giriş/refresh
- Kullanıcı favorileri
- Profil ve giriş ekranları
- Rol tabanlı güvenlik temeli


## Faz 23–28
Tam admin operasyonları, CSV veri hattı, Supabase indeksleri, admin frontend, CI kalite kapıları ve production kontrol listesi eklendi. Ayrıntılar: `docs/PHASES-23-28.md`.

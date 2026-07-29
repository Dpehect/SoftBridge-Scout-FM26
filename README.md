# FM26 Scout Platform

Portföy ve kurumsal kullanım seviyesinde Football Manager 2026 scout platformu.

## Teknoloji

- Next.js 16 + React 19 + TypeScript
- ASP.NET Core Web API (.NET 10)
- Neon PostgreSQL + Entity Framework Core
- JWT authentication ve role-based authorization
- Docker
- Vercel frontend deployment
- Koyeb backend deployment

## Production mimarisi

```text
Frontend  -> Vercel
Backend   -> Koyeb
Database  -> Neon PostgreSQL
```

## Local backend

```bash
dotnet run --project src/FmScout.Api/FmScout.Api.csproj
```

Local PostgreSQL bağlantısını ortam değişkeniyle tanımlayın:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=fm26scout;Username=postgres;Password=postgres"
```

## Local frontend

```bash
cd apps/web
npm install
npm run dev
```

## Production deployment

Koyeb ve Neon kurulum adımları:

```text
docs/KOYEB-NEON-DEPLOYMENT.md
```

Gerçek bağlantı dizeleri ve gizli anahtarlar GitHub dosyalarına eklenmez. Bunlar hosting panellerindeki environment variables alanlarından yönetilir.

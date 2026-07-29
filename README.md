# FM26 Scout Platform

Portföy seviyesinde Football Manager 2026 scout platformu.

## Teknoloji

- Next.js 16 + React 19 + TypeScript
- ASP.NET Core Web API (.NET 10)
- PostgreSQL + Entity Framework Core
- JWT authentication ve role-based authorization
- Docker
- Vercel frontend deployment
- Render Blueprint backend ve database deployment

## Render ile deploy

Ayrıntılı ve adım adım kurulum:

```text
docs/RENDER-DEPLOYMENT.md
```

Render tek `render.yaml` dosyası üzerinden API ve PostgreSQL veritabanını birlikte oluşturur.

## Local backend

```bash
dotnet run --project src/FmScout.Api/FmScout.Api.csproj
```

## Local frontend

```bash
cd apps/web
npm install
npm run dev
```

## Render build fix

The Application project explicitly references `Microsoft.Extensions.DependencyInjection.Abstractions` so its `AddApplication(IServiceCollection)` extension compiles independently under .NET 10.

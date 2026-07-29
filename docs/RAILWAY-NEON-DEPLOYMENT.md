# Railway + Neon Deployment

## Mimari

```text
Next.js frontend -> Vercel
ASP.NET Core API -> Railway
PostgreSQL -> Neon
```

## Railway kurulumu

1. Railway hesabına GitHub ile giriş yap.
2. **New Project** > **Deploy from GitHub repo** seç.
3. `Dpehect/SoftBridge-Scout-FM26` reposunu seç.
4. Railway kökteki `railway.json` ve `Dockerfile` dosyalarını kullanır.
5. Servis açılınca **Variables** bölümüne aşağıdaki değerleri ekle:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<NEON_POOLER_CONNECTION_STRING>
Cors__Origins__0=<VERCEL_PRODUCTION_URL>
Jwt__Issuer=fm26-scout-api
Jwt__Audience=fm26-scout-web
Jwt__Secret=<EN_AZ_64_KARAKTER_RASTGELE_DEGER>
Jwt__AccessMinutes=30
Admin__ApiKey=<EN_AZ_32_KARAKTER_RASTGELE_DEGER>
```

Gerçek şifreleri veya bağlantı dizelerini GitHub dosyalarına yazma.

## Domain oluşturma

Railway servisinde:

1. **Settings** > **Networking** bölümüne gir.
2. **Generate Domain** seç.
3. Oluşan adresi kopyala.

Örnek:

```text
https://fm26-scout-production.up.railway.app
```

## Doğrulama

Health endpoint:

```text
https://<RAILWAY_DOMAIN>/health
```

API istekleri `/api` altındadır.

## Vercel bağlantısı

Vercel projesinde aşağıdaki environment variable'ı ekle:

```text
NEXT_PUBLIC_API_URL=https://<RAILWAY_DOMAIN>/api
```

Sonra Vercel projesini yeniden deploy et.

## Güvenlik

- Neon connection string'i yalnızca Railway Variables alanında tut.
- JWT secret en az 64 karakter olsun.
- Admin API key en az 32 karakter olsun.
- Secret değerlerini ekran görüntüsünde paylaşma.

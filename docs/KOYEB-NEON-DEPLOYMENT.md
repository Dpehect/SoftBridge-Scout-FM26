# Koyeb + Neon Deployment

## Mimari

```text
Next.js frontend -> Vercel
ASP.NET Core API -> Koyeb
PostgreSQL -> Neon
```

## 1. Neon

Neon projesinde:

- Region: Europe / Frankfurt
- Branch: production
- Database: neondb
- Connection pooling: açık

Neon bağlantı dizesini yalnızca Koyeb environment variable olarak kullanın. GitHub'a eklemeyin.

## 2. Koyeb service

GitHub repository:

```text
Dpehect/SoftBridge-Scout-FM26
```

Deployment ayarları:

```text
Builder: Dockerfile
Dockerfile path: Dockerfile
Port: 10000
Health check path: /health
```

## 3. Koyeb environment variables

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

Neon URL örneği:

```text
postgresql://USER:PASSWORD@HOST-pooler.eu-central-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require
```

Gerçek kullanıcı adı, şifre veya bağlantı dizesini repoya yazmayın.

## 4. Frontend environment variable

Vercel'de frontend'in kullandığı API URL değişkenini Koyeb adresine ayarlayın. Projedeki mevcut değişken adına göre örnek:

```text
NEXT_PUBLIC_API_URL=https://<KOYEB-SERVICE>.koyeb.app
```

Ardından Vercel deployment'ını yeniden başlatın.

## 5. Doğrulama

API kökü:

```text
https://<KOYEB-SERVICE>.koyeb.app/
```

Health endpoint:

```text
https://<KOYEB-SERVICE>.koyeb.app/health
```

İlk açılışta uygulama Neon veritabanını oluşturur ve başlangıç verilerini yükler.

## Güvenlik

- Neon parolası ekran görüntüsünde görünürse Neon üzerinden sıfırlayın.
- JWT secret ve Admin API key için uzun, rastgele değerler kullanın.
- Secret değerlerini README, appsettings veya GitHub Actions loglarına koymayın.

# Render + Neon düzeltmesi

Bu paketteki dosyaları proje köküne kopyalayın ve mevcut dosyaların üzerine yazın.

## Render Environment Variables

Aşağıdaki değişkenler zorunludur:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<NEON CONNECTION STRING>
Cors__Origins__0=https://<VERCEL-DOMAIN>
Jwt__Issuer=fm26-scout-api
Jwt__Audience=fm26-scout-web
Jwt__Secret=<EN AZ 32 KARAKTER>
Jwt__AccessMinutes=30
Admin__ApiKey=<EN AZ 32 KARAKTER>
```

Neon bağlantısını Neon panelindeki **Connect** alanından alın. Tercihen pooled bağlantıyı kullanın.
Bağlantı `postgresql://...` biçiminde olabilir.

Önemli:
- Değişken adı tam olarak `ConnectionStrings__DefaultConnection` olmalıdır.
- Değere tırnak eklemeyin.
- Sonunda boşluk bırakmayın.
- `localhost` veya `127.0.0.1` içeren bağlantı kullanmayın.
- Vercel adresinin sonuna `/` koymayın.

Sonra Render:
1. Manual Deploy
2. Clear build cache & deploy
3. `https://<render-domain>/health` adresini kontrol edin.

Vercel:
```text
NEXT_PUBLIC_API_URL=https://<render-domain>/api
```

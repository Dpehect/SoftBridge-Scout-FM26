# Production Checklist

1. Supabase SQL Editor'da `supabase/migrations/0001_bootstrap.sql` çalıştır.
2. Render environment değerlerini ekle: `ConnectionStrings__DefaultConnection`, `Jwt__Secret`, `Admin__Key`, `Cors__Origins__0`.
3. Render health check yolunu `/health` yap.
4. Vercel root directory değerini `apps/web` yap.
5. Vercel'e `NEXT_PUBLIC_API_URL=https://RENDER-URL/api` ekle.
6. İlk deployment sonrası `/health`, `/api/scouting/stats`, `/players` ve `/admin` sayfalarını kontrol et.
7. Admin anahtarını yalnızca Render'da tut; `NEXT_PUBLIC_` değişkenine yazma.
8. Gerçek oyuncu verisini yalnızca kullanım hakkın olan kaynaklardan içe aktar.

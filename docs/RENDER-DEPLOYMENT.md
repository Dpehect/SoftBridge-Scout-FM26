# Render Deployment — FM26 Scout Platform

Bu paket backend API ve PostgreSQL veritabanını tek Render Blueprint ile oluşturur.
Frontend Vercel'de kalır.

## 1. Dosyaları GitHub'a yükle

ZIP içeriğini `Dpehect/SoftBridge-Scout-FM26` reposunun köküne yükle.
Repo kökünde `render.yaml` görünmelidir.

## 2. Render hesabını GitHub'a bağla

1. Render Dashboard'a gir.
2. **New +** seç.
3. **Blueprint** seç.
4. GitHub hesabını bağla.
5. `SoftBridge-Scout-FM26` reposunu seç.
6. Branch olarak `main` seç.
7. Blueprint dosyası olarak `render.yaml` kullanılmalıdır.

## 3. Frontend adresini gir

Render kurulum sırasında `Cors__Origins__0` değerini sorar.
Buraya Vercel frontend adresini eksiksiz gir:

```text
https://projen.vercel.app
```

Sonunda `/` kullanma.

## 4. Apply

**Apply** düğmesine bas. Render otomatik olarak iki kaynak oluşturur:

- `fm26-scout-api`
- `fm26-scout-db`

İlk Docker build birkaç dakika sürebilir.

## 5. API adresini al

API servisi açıldığında üst bölümde şu tip adres görünür:

```text
https://fm26-scout-api.onrender.com
```

Kontrol et:

```text
https://fm26-scout-api.onrender.com/health
```

Sağlıklı yanıt alınırsa API çalışıyordur.

## 6. Vercel environment variable

Vercel projesinde:

**Settings → Environment Variables**

şu değişkeni ekle:

```text
NEXT_PUBLIC_API_URL=https://fm26-scout-api.onrender.com/api
```

Production, Preview ve Development ortamlarını seç. Sonra Vercel'de yeniden deploy et.

## 7. Render CORS güncellemesi

Vercel domainin değişirse:

1. Render → `fm26-scout-api`
2. **Environment**
3. `Cors__Origins__0` değerini yeni Vercel adresiyle değiştir
4. **Save Changes**

Render servisi otomatik yeniden deploy edilir.

## 8. Otomatik deploy

`main` branch'e yeni commit gönderildiğinde Render backend'i otomatik build ve deploy eder.
Vercel de frontend'i otomatik deploy eder.

## Hata kontrolü

Render API servisinde **Logs** sekmesini aç.

Yaygın durumlar:

- `ConnectionStrings:DefaultConnection is required`: Blueprint yerine manuel servis kurulmuştur veya database bağlantısı eksiktir.
- CORS hatası: `Cors__Origins__0` Vercel adresiyle birebir eşleşmiyordur.
- İlk istek yavaş: ücretsiz Render web service 15 dakika boş kalınca uyur.
- Veritabanı erişilemiyor: database henüz hazır değildir veya deploy tekrar başlatılmalıdır.

## Ücretsiz plan uyarısı

Render'ın ücretsiz PostgreSQL veritabanı 30 gün sonra sona erer. Kalıcı portföy kullanımı için daha sonra veritabanını ücretli plana yükseltmek veya dış PostgreSQL servisine taşımak gerekir.

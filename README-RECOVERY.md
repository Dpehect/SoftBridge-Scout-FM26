# FM26 Scout güvenli backend kurtarma paketi
Bu paket mevcut çalışan Render yapısını korur ve Neon PostgreSQL destekli gerçek API katmanını geri ekler.

## Kontrol adresleri
- `/health`: servis liveness
- `/health/db`: Neon bağlantısı
- `/api/status`: API durumu
- `/api/players`: oyuncular
- `/api/scout-reports`: scout raporları

Connection string kaynak koda gömülmez; Render environment variable üzerinden okunur.

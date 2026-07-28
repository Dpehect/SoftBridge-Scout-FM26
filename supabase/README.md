# Supabase kurulumu

1. Supabase üzerinde ücretsiz proje oluştur.
2. **Project Settings → Database → Connection string → Session pooler** adresini al.
3. URI bağlantısını Render'da `ConnectionStrings__DefaultConnection` değişkenine ekle.
4. Backend ilk açılışta şemayı ve örnek verileri güvenli şekilde oluşturur.

Örnek Npgsql formatı:

```text
Host=aws-0-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.PROJECT_REF;Password=PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=15;Timeout=15;Command Timeout=30
```

Veritabanı parolası yalnızca Render'da saklanmalıdır.

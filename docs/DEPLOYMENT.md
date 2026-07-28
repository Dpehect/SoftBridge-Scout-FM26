# Ücretsiz deployment

## 1. GitHub
ZIP içeriğini repo köküne yükle.

## 2. Supabase
Yeni proje oluştur, Database Settings içinden Session Pooler bağlantısını kopyala.

## 3. Render
New → Blueprint → GitHub reposu → `render.yaml`. Gizli connection string ve Vercel origin değerini ekle.

## 4. Vercel
New Project → aynı repo → Root Directory `apps/web` → `NEXT_PUBLIC_API_URL` ekle.

## 5. Sıralama
Önce Supabase, sonra Render, en son Vercel kurulmalıdır.

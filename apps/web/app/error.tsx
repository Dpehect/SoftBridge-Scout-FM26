"use client";

import { useEffect } from "react";

export default function ErrorPage({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("Application runtime error:", error);
  }, [error]);

  return (
    <main className="container page">
      <div className="panel" style={{ padding: 32, maxWidth: 720 }}>
        <span className="eyebrow">UYGULAMA HATASI</span>
        <h1>Sayfa yüklenirken bir hata oluştu.</h1>
        <p style={{ color: "var(--muted)" }}>
          {error.message || "Bilinmeyen çalışma zamanı hatası."}
        </p>
        {error.digest ? (
          <p style={{ color: "var(--muted)", fontSize: 13 }}>
            Hata kodu: {error.digest}
          </p>
        ) : null}
        <button className="btn btn-primary" onClick={reset}>
          Tekrar dene
        </button>
      </div>
    </main>
  );
}

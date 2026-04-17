# KOAFiloServis v1.0.2

**Tarih:** 2026-04-17
**Paket:** `KOAFiloServisKurulum-1.0.2.exe` (~147 MB)

## 🎯 Ana Hedef
v1.0.1'de test PC'de kurulum başarılı görünüyor ama `http://localhost:5190` IIS üzerinden açılmıyordu. v1.0.2 bu sorunu **kalıcı olarak** çözüyor: eksik bileşen varsa kurulum hiç başlamıyor; bileşen varsa kurulum sonunda smoke test ile IIS'in gerçekten çalıştığı doğrulanıyor.

## ✨ Yenilikler

### Kurulum Ön Kontrolü (InitializeSetup)
Kurulum **başlamadan önce** aşağıdaki bileşenler kontrol edilir; eksikse kullanıcıya hangi bileşenlerin gerektiği gösterilir ve kurulum iptal edilir (dosyalar diske hiç açılmaz):
- IIS (Internet Information Services)
- ASP.NET Core Module V2
- .NET 10 ASP.NET Core Runtime

### IIS Otomasyonu Sertleştirildi
- **`iisreset /noforce`** — Hosting Bundle yeni yüklendiyse modül cache yenilenir.
- **Port fallback** — 5190 başka process tarafından kullanılıyorsa otomatik olarak 5191..5194 denenir. Kullanılan port `C:\KOAFiloServis\active-port.txt`'ye yazılır.
- **Smoke test** — Kurulumdan sonra `Invoke-WebRequest http://localhost:<port>` ile uygulamanın gerçekten yanıt verdiği doğrulanır (20 sn timeout).
- **Binding temizliği** — Upgrade sırasında eski HTTP binding'ler otomatik temizlenir.
- **AppPool sertleştirme** — `enable32BitAppOnWin64=false`, `startMode=AlwaysRunning`, `autoStart=true`.

### Tanı Kolaylığı
- `web.config` içinde `stdoutLogEnabled="true"` — IIS ilk çalıştırma sorunları `C:\KOAFiloServis\logs\stdout*.log` dosyalarından izlenebilir.
- Smoke test fail olursa stdout log son 30 satırı doğrudan kurulum console'una basılır.

## 🔄 Yükseltme
v1.0.0 / v1.0.1 üzerine doğrudan kurulabilir. Kullanıcı verileri (`data\*.db`, `uploads\`, `logs\`, `Backups\`, `dbsettings.json`) korunur.

## ⚠️ Ön Gereksinimler
Test PC'de aşağıdakiler kurulu olmalıdır (yoksa kurulum sihirbazı bilgi verip çıkar):
1. **IIS** — Windows Özellikleri → "Internet Information Services"
2. **.NET 10 Hosting Bundle** — https://dotnet.microsoft.com/download/dotnet/10.0

## 🔍 SHA256
`<sha256-hash-buraya>`

## 📚 Dokümantasyon
- Kurulum adımları: `setup/README.md`
- Bu paketle birlikte gelen yardımcı araç: `C:\KOAFiloServis\DataSync\KOAFiloServis.DataSync.exe` (PostgreSQL → SQLite veri aktarımı)

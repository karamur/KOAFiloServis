# Tenant (Firma) Mimarisi - Tam Yeniden Yapılandırma

> **Amaç:** Zirve Müşavirlik mantığı. Kullanıcı login olunca firma seçer; o oturum boyunca tüm CRUD/hesaplama
> sadece o firma verisi üzerinde döner. Firmalar birbirine **sızmaz**. İstenirse şirketler arası kopyalama
> (toplu/tekil) ve şirketler arası kasa/banka transferi yapılabilir.
>
> **Dokunulmayacak modüller:** Bütçe, Muhasebe. Bu modüllerin entity'leri global filter'dan muaftır.

---

## Karar Listesi (Sabit, değişmez referans)

| # | Karar |
|---|------|
| K1 | Tek tenant kavramı: `Firma`. Eski `Sirket` / `SirketId` / `TenantService` deprecated. Veri kaybı olmasın diye hemen drop edilmez, aşamalı emekliliğe alınır. |
| K2 | Aktif firma: Blazor Server **scoped** servis (`IAktifFirmaProvider`) + Session cookie. `FirmaService` içindeki `static _aktifFirma` **bug** → düzeltilecek. |
| K3 | `ApplicationDbContext` global query filter (`HasQueryFilter`) → `FirmaId == aktif` otomatik. Servislerde `.Where(FirmaId == ...)` yazılmaz. |
| K4 | `IFirmaTenant` marker interface. `FirmaId` taşıyan tüm entity'ler implemente eder. |
| K5 | Araç sahiplik 3 tip: `Ozmal`, `Kiralik` (kira firmaya gider), `Tedarikci` (masraf tedarikçide; **lastik + belge takip her zaman firmada**). |
| K6 | Kasa/Banka firma bazlı. Şirketler arası transfer ayrı entity (`FirmalarArasiTransfer`). |
| K7 | Bütçe + Muhasebe dokunulmaz, global filter'dan muaf. |
| K8 | Şirketler arası kopyalama: yeni kayıt üretir, `KaynakFirmaId + KaynakKayitId` audit. Hareketler kopyalanmaz, sadece master kartlar. |
| K9 | Migration: kolon nullable ekle → default firma ile doldur → `IsRequired()`'a al. Veri kaybı yok. |

---

## Aşama Durum Tablosu

| Aşama | Açıklama | Durum | Commit/Migration |
|------|----------|------|------------------|
| A | Plan + IFirmaTenant + IAktifFirmaProvider + FirmaService bug fix | ✅ tamam | (commit edilecek) |
| B | Firma.CariId kaldır, Cari.SirketId deprecate, DbContext global filter | ✅ tamam | (commit edilecek) |
| C | Master entity'lere FirmaId zorunlu (Cari, Kurum, Guzergah, Sofor, Arac, BankaHesap, Stok, MasrafKalemi…) | ⏳ devam (C1 tamam) | TenantC1_AddFirmaIdToMasterEntities |
| D | AracSahiplikTipi sadeleştirme + masraf sahibi helper | ✅ tamam | (commit edilecek) |
| E | Kasa/Banka firma bazlı + FirmalarArasiTransfer | ✅ tamam (UI E2'ye ertelendi) | TenantE1_AddFirmaIdToBankaKasaAndFirmalarArasiTransfer |
| F | FirmaKopyalamaService + UI (toplu/tekil checkbox) | ✅ tamam (servis + UI + migration) | TenantF1_AddKopyalanabilirTenantAuditColumns |
| G | Hakediş Puantaj ekranı (Excel benzeri tablo) | ⬜ bekliyor | - |
| H | Login sonrası firma seçim ekranı + üst bar firma değiştirici | ✅ tamam | - |

---

## Aşama A — Yapılacaklar Detay (TAMAM)

- [x] `docs/TENANT_MIGRATION_PLAN.md` (bu dosya)
- [x] `KOAFiloServis.Shared/Entities/IFirmaTenant.cs` — marker interface
- [x] `KOAFiloServis.Web/Services/IAktifFirmaProvider.cs` + `AktifFirmaProvider` impl (scoped)
- [x] `FirmaService` artık `static _aktifFirma` kullanmıyor, provider'a delege ediyor
- [x] `Program.cs`'te `IAktifFirmaProvider` ve `FirmaService` **Scoped** kaydı (eskiden Singleton'dı)
- [x] `dotnet build` geçiyor

## Aşama B — Yapılacaklar Detay (TAMAM)

- [x] `Firma.CariId` `[Obsolete]` işaretlendi (kolon henüz drop edilmedi; veri güvenliği için Aşama F sonrasına ertelendi)
- [x] `Cari.SirketId` ve `Cari.Sirket` `[Obsolete]` işaretlendi (legacy `Sirket` yapısı ileride emekliye)
- [x] `TenantFilterIgnoreAttribute` eklendi (Bütçe/Muhasebe muafiyeti için)
- [x] `ApplicationDbContext` artık `IAktifFirmaProvider`'ı lazy resolve ediyor (`ResolveAktifFirmaProvider`)
- [x] `IFirmaTenant` entity'lere otomatik named query filter (`"Tenant"`) eklendi (`ApplyFirmaTenantQueryFilter`)
- [x] `SaveChanges` / `SaveChangesAsync` artık yeni eklenen `IFirmaTenant` kayıtlarına aktif `FirmaId`'yi otomatik atıyor (`AssignFirmaTenantId`)
- [x] `dotnet build` geçiyor (0 error, 54 obsolete warning — hepsi planlı temizlik)

## Aşama C — Master Entity FirmaId Listesi

### C1 — Marker interface + nullable FirmaId (TAMAM)

| Entity | Durum | Not |
|--------|-------|-----|
| Kurum | ✅ IFirmaTenant + FirmaId (yeni kolon) | Migration `TenantC1_AddFirmaIdToMasterEntities` |
| Guzergah | ✅ IFirmaTenant (FirmaId zaten vardı) | SirketId `[Obsolete]` |
| Arac | ✅ IFirmaTenant + FirmaId (yeni kolon) | SirketId `[Obsolete]` |
| Sofor | ✅ IFirmaTenant (FirmaId zaten vardı) | SirketId `[Obsolete]` |
| Cari | ✅ IFirmaTenant (FirmaId zaten vardı) | SirketId `[Obsolete]` (Aşama B) |

### C2 — Veri doldurma + NOT NULL (bekliyor)

| Entity | Durum | Not |
|--------|-------|-----|
| Kurum | ⬜ | NULL kayıtlara varsayılan firma ata, sonra `IsRequired()` |
| Guzergah | ⬜ | aynı |
| Arac | ⬜ | aynı |
| Sofor | ⬜ | aynı |
| Cari | ⬜ | aynı |

### C3 — Kalan entity'ler (bekliyor)

| Entity | Şu an FirmaId? | Yapılacak |
|--------|----------------|-----------|
| BankaHesap | kontrol | Aşama E içinde zaten yapılacak |
| BankaKasaHareket | kontrol | Aşama E içinde |
| Stok | kontrol | C3 |
| MasrafKalemi | kontrol | C3 |
| Fatura | kontrol | C3 (Cari üzerinden gelir ama explicit olsun) |
| ServisCalisma | kontrol | C3 |

---

## Yarıda Kaldıysak Buradan Devam

1. Bu dosyadaki **Aşama Durum Tablosu**'na bak.
2. `⏳ devam` olan aşamanın "Yapılacaklar Detay" listesindeki ilk işaretsiz maddeden başla.
3. Aşama bitince satırını `✅ tamam` yap, commit at, bir sonraki aşamayı `⏳ devam` yap.
4. Veri kaybı olmaması için Aşama B-C'deki migration sırasını **bozma** (nullable → doldur → required).

### Şu Anki Devam Noktası (Aşama G — Hakediş Puantaj ekranı)

**Aşama H tamam:**

- Yeni sayfa `Pages/FirmaSec.razor` (route: `/firma-sec`, layout: `EmptyLayout`, `[Authorize]`).
  - Aktif firmaları kart şeklinde listeler; tıklanınca `FirmaService.SetAktifFirma(id)` + `SetTumFirmalar(false)` çağrılır ve `returnUrl`'e yönlendirir.
  - **Tek firma varsa otomatik seçer ve hemen yönlendirir** (fazladan tıklama olmaz).
  - "Tüm Firmalar (raporlama)" butonu da var.
- `Login.razor` → `GetSafeReturnUrl()` artık `/firma-sec?returnUrl=<gercekHedef>` döndürüyor. Standart, 2FA ve replay (yeniden render) akışlarının hepsi bu metoda düşüğü için tek noktadan geçiyor.
- `MainLayout.razor` üst bar firma dropdown'una **"Firma Değiştir (Tam Ekran)"** linki eklendi.
- `MainLayout.razor` Ayarlar dropdown'una **"Şirketler Arası Kopyalama"** linki eklendi (Aşama F'in UI giriş noktası).
- `FirmaSelector.razor` ve mevcut layout firma dropdown'u zaten scoped `IAktifFirmaProvider` üzerinden çalışıyor (Aşama A); Aşama H'de ekstra değişiklik gerekmedi.
- `dotnet build` 0 error.

**Not (kalıcılık):** Aktif firma şu an per-circuit in-memory. Tarayicinin kapanıp açılmasında `AktifFirmaProvider` reset olur ve `FirmaService.GetAktifFirma()` varsayılan firmaya düşer; bu davranış şimdilik kabul edildi. Session cookie kalıcılığı (K2'nin ikinci yarısı) gerekirse ileride `ProtectedLocalStorage` ile eklenebilir.

**Aşama G hedefi:** Hakediş puantaj ekranı (Excel benzeri tablo). Firma bazlı çalışacak (aktif firma + dönem üzerinden).

**Başlamadan önce yap:** Aşama H commit + push.
```
git add docs/TENANT_MIGRATION_PLAN.md \
        KOAFiloServis.Web/Components/Pages/FirmaSec.razor \
        KOAFiloServis.Web/Components/Pages/Login.razor \
        KOAFiloServis.Web/Components/Layout/MainLayout.razor
git commit -m "tenant: Aşama H - Login sonrası firma seçim ekranı + menü linkleri (K2)"
git push origin main
```

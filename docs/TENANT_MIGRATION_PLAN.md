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
| E | Kasa/Banka firma bazlı + FirmalarArasiTransfer | ⏳ devam | - |
| F | FirmaKopyalamaService + UI (toplu/tekil checkbox) | ⬜ bekliyor | - |
| G | Hakediş Puantaj ekranı (Excel benzeri tablo) | ⬜ bekliyor | - |
| H | Login sonrası firma seçim ekranı + üst bar firma değiştirici | ⬜ bekliyor | - |

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

### Şu Anki Devam Noktası (Aşama E — Kasa/Banka firma bazlı + transfer)

**Aşama D tamamlandı** (commit atılacak):

- `KOAFiloServis.Web/Services/AracMasrafSahibiHelper.cs` eklendi: `GetMasrafSahibi(arac, kategori)`, `GetBelgeTakipSahibi`, `GetSoforMasrafSahibi`, `HesaplaCPlakaKirasi`.
- K5 kuralı kodlanı: lastik + belge her zaman firmada; kiralıkta C plaka kirası firmada; tedarikçide operasyonel masraf tedarikçide.
- `SahiplikHelper.cs` açıklamaları K5 ile hizalandı (Kiralık ve Tedarikçi).
- `AracSahiplikTipi` enum'una **dokunulmadı**: `Komisyon` ve `Diger` zaten kullanımda; veri kaybı riski oluştururdu. Helper içinde Komisyon=Firma, Diger=Firma davranıyor.

**Aşama E hedefi:** Kasa/Banka kayıtlarını firma bazlı izole et + `FirmalarArasiTransfer` entity ile şirketler arası transfer (K6).

**Sıradaki adımlar:**

1. `BankaHesap` ve `BankaKasaHareket` entity'lerine `IFirmaTenant` implement et + nullable `FirmaId` (zaten varsa atla).
2. EF migration: `TenantE1_AddFirmaIdToBankaKasa`.
3. Yeni entity: `FirmalarArasiTransfer` (KaynakFirmaId, HedefFirmaId, KaynakHesapId, HedefHesapId, Tutar, Tarih, Aciklama, OlusturulanHareketKaynakId, OlusturulanHareketHedefId).
4. `FirmalarArasiTransferService` — tek transferden iki `BankaKasaHareket` kaydı üret (kaynakta -Tutar, hedefte +Tutar), her ikisinin `FirmaId`'si doğru kalmalı (manuel set, tenant filter atlanmadan).
5. UI: `Pages/KasaBanka/FirmalarArasiTransfer.razor` (basit form).

**Başlamadan önce yap:** Aşama D commit + push.
```
git add docs/TENANT_MIGRATION_PLAN.md \
        KOAFiloServis.Web/Services/AracMasrafSahibiHelper.cs \
        KOAFiloServis.Web/Helpers/SahiplikHelper.cs
git commit -m "tenant: Aşama D - AracMasrafSahibiHelper + K5 açıklama hizalama"
git push origin main
```

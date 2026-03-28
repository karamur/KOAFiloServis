using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRMFiloServis.Shared;

namespace CRMFiloServis.LisansDesktop;

public partial class Form1 : Form
{
    private const string LisansAnahtar = "CRMFiloServis2026SecretKey!@";
    private readonly string _kayitDosyaYolu;
    private List<LisansTakipKaydi> _kayitlar = new();
    private Guid? _seciliKayitId;

    public Form1()
    {
        InitializeComponent();
        cmbLisansTipi.SelectedIndex = 1;
        cmbIslemTipi.SelectedIndex = 0;
        dtpBitisTarihi.Value = DateTime.Today.AddYears(1);

        var klasor = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRMFiloServis", "LisansOlusturucu");
        Directory.CreateDirectory(klasor);
        _kayitDosyaYolu = Path.Combine(klasor, "lisans_kayitlari.json");

        KayitlariYukle();
        GridiDoldur();
    }

    private void btnBuPcMakineKodu_Click(object? sender, EventArgs e)
    {
        txtMakineKodu.Text = LisansHelper.GetMachineCode();
    }

    private void btnLisansUret_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtFirmaAdi.Text))
            {
                MessageBox.Show("Firma adı zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirmaAdi.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMakineKodu.Text))
            {
                MessageBox.Show("Makine kodu zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMakineKodu.Focus();
                return;
            }

            var islemTipi = cmbIslemTipi.SelectedItem?.ToString() ?? "Yeni Kayıt";
            var oncekiKayit = _seciliKayitId.HasValue ? _kayitlar.FirstOrDefault(x => x.Id == _seciliKayitId.Value) : null;
            var takipNo = oncekiKayit?.TakipNo ?? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

            if (islemTipi != "Yeni Kayıt")
            {
                var bagliKayit = oncekiKayit ?? _kayitlar
                    .Where(x => x.MakineKodu.Equals(txtMakineKodu.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.IslemTarihi)
                    .FirstOrDefault();

                if (bagliKayit == null)
                {
                    MessageBox.Show("Yenileme/Güncelleme için önce mevcut bir lisans kaydı seçin veya aynı makine koduna ait kayıt bulunmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                takipNo = bagliKayit.TakipNo;
            }

            var lisans = new DesktopLisansBilgi
            {
                LisansKodu = UretLisansKodu(),
                FirmaAdi = txtFirmaAdi.Text.Trim(),
                YetkiliKisi = txtYetkili.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Telefon = txtTelefon.Text.Trim(),
                LisansTipi = cmbLisansTipi.SelectedItem?.ToString() ?? "Standard",
                BaslangicTarihi = DateTime.UtcNow,
                BitisTarihi = DateTime.SpecifyKind(dtpBitisTarihi.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc),
                MaxKullaniciSayisi = (int)numMaxKullanici.Value,
                MaxAracSayisi = (int)numMaxKullanici.Value * 10,
                MakineKodu = txtMakineKodu.Text.Trim(),
                Aktif = true
            };

            var lisansJson = JsonSerializer.Serialize(lisans);
            var aktivasyonKodu = EncryptString(lisansJson);
            txtLisansAnahtari.Text = aktivasyonKodu;

            if (islemTipi != "Yeni Kayıt")
            {
                foreach (var kayit in _kayitlar.Where(x => x.TakipNo == takipNo && x.Aktif))
                {
                    kayit.Aktif = false;
                }
            }

            var yeniKayit = new LisansTakipKaydi
            {
                Id = Guid.NewGuid(),
                TakipNo = takipNo,
                OncekiKayitId = oncekiKayit?.Id,
                IslemTipi = islemTipi,
                IslemTarihi = DateTime.UtcNow,
                LisansKodu = lisans.LisansKodu,
                AktivasyonKodu = aktivasyonKodu,
                FirmaAdi = lisans.FirmaAdi,
                YetkiliKisi = lisans.YetkiliKisi,
                Email = lisans.Email,
                Telefon = lisans.Telefon,
                LisansTipi = lisans.LisansTipi,
                BaslangicTarihi = lisans.BaslangicTarihi,
                BitisTarihi = lisans.BitisTarihi,
                MaxKullaniciSayisi = lisans.MaxKullaniciSayisi,
                MaxAracSayisi = lisans.MaxAracSayisi,
                MakineKodu = lisans.MakineKodu,
                Aktif = true,
                Notlar = txtNotlar.Text.Trim()
            };

            _kayitlar.Add(yeniKayit);
            KayitlariKaydet();
            GridiDoldur();
            _seciliKayitId = yeniKayit.Id;

            MessageBox.Show("Lisans aktivasyon kodu oluşturuldu ve kayıt altına alındı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnKopyala_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLisansAnahtari.Text))
        {
            MessageBox.Show("Önce lisans üretin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Clipboard.SetText(txtLisansAnahtari.Text);
        MessageBox.Show("Lisans kodu panoya kopyalandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnTxtKaydet_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLisansAnahtari.Text))
        {
            MessageBox.Show("Önce lisans üretin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        saveFileDialog1.FileName = $"lisans_{txtFirmaAdi.Text.Trim().Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.txt";
        if (saveFileDialog1.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        File.WriteAllText(saveFileDialog1.FileName, txtLisansAnahtari.Text, Encoding.UTF8);
        MessageBox.Show("Lisans dosyası kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnSeciliKaydiYukle_Click(object? sender, EventArgs e)
    {
        if (!_seciliKayitId.HasValue)
        {
            MessageBox.Show("Önce listeden bir kayıt seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var kayit = _kayitlar.FirstOrDefault(x => x.Id == _seciliKayitId.Value);
        if (kayit == null)
        {
            return;
        }

        txtFirmaAdi.Text = kayit.FirmaAdi;
        txtYetkili.Text = kayit.YetkiliKisi;
        txtEmail.Text = kayit.Email;
        txtTelefon.Text = kayit.Telefon;
        txtMakineKodu.Text = kayit.MakineKodu;
        cmbLisansTipi.SelectedItem = kayit.LisansTipi;
        numMaxKullanici.Value = Math.Min(numMaxKullanici.Maximum, Math.Max(numMaxKullanici.Minimum, kayit.MaxKullaniciSayisi));
        dtpBitisTarihi.Value = kayit.BitisTarihi.ToLocalTime().Date;
        txtNotlar.Text = kayit.Notlar ?? string.Empty;
        txtLisansAnahtari.Text = kayit.AktivasyonKodu;
        cmbIslemTipi.SelectedItem = "Yenileme";
    }

    private void btnYeniForm_Click(object? sender, EventArgs e)
    {
        _seciliKayitId = null;
        txtFirmaAdi.Clear();
        txtYetkili.Clear();
        txtEmail.Clear();
        txtTelefon.Clear();
        txtMakineKodu.Clear();
        txtNotlar.Clear();
        txtLisansAnahtari.Clear();
        cmbLisansTipi.SelectedIndex = 1;
        cmbIslemTipi.SelectedIndex = 0;
        numMaxKullanici.Value = 10;
        dtpBitisTarihi.Value = DateTime.Today.AddYears(1);
        dgvKayitlar.ClearSelection();
    }

    private void dgvKayitlar_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvKayitlar.CurrentRow?.DataBoundItem is LisansTakipListeSatiri satir)
        {
            _seciliKayitId = satir.Id;
        }
    }

    private void KayitlariYukle()
    {
        if (!File.Exists(_kayitDosyaYolu))
        {
            _kayitlar = new List<LisansTakipKaydi>();
            return;
        }

        var json = File.ReadAllText(_kayitDosyaYolu, Encoding.UTF8);
        _kayitlar = JsonSerializer.Deserialize<List<LisansTakipKaydi>>(json) ?? new List<LisansTakipKaydi>();
    }

    private void KayitlariKaydet()
    {
        var json = JsonSerializer.Serialize(_kayitlar.OrderByDescending(x => x.IslemTarihi).ToList(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_kayitDosyaYolu, json, Encoding.UTF8);
    }

    private void GridiDoldur()
    {
        var liste = _kayitlar
            .OrderByDescending(x => x.IslemTarihi)
            .Select(x => new LisansTakipListeSatiri
            {
                Id = x.Id,
                TakipNo = x.TakipNo,
                FirmaAdi = x.FirmaAdi,
                IslemTipi = x.IslemTipi,
                LisansTipi = x.LisansTipi,
                MaxKullanici = x.MaxKullaniciSayisi,
                Baslangic = x.BaslangicTarihi.ToLocalTime().ToString("dd.MM.yyyy"),
                Bitis = x.BitisTarihi.ToLocalTime().ToString("dd.MM.yyyy"),
                KalanGun = Math.Max(0, (x.BitisTarihi.Date - DateTime.UtcNow.Date).Days),
                Aktif = x.Aktif ? "Evet" : "Hayır"
            })
            .ToList();

        dgvKayitlar.DataSource = null;
        dgvKayitlar.DataSource = liste;

        var idColumn = dgvKayitlar.Columns[nameof(LisansTakipListeSatiri.Id)];
        if (idColumn != null)
        {
            idColumn.Visible = false;
        }
    }

    private static string UretLisansKodu()
    {
        var random = new Random();
        return $"CRM-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}";
    }

    private static string EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(LisansAnahtar));
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        var iv = aes.IV;
        var encrypted = msEncrypt.ToArray();
        var result = new byte[iv.Length + encrypted.Length];

        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }

    private sealed class DesktopLisansBilgi
    {
        public string LisansKodu { get; set; } = string.Empty;
        public string FirmaAdi { get; set; } = string.Empty;
        public string YetkiliKisi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string LisansTipi { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int MaxKullaniciSayisi { get; set; }
        public int MaxAracSayisi { get; set; }
        public string MakineKodu { get; set; } = string.Empty;
        public bool Aktif { get; set; }
    }

    private sealed class LisansTakipKaydi
    {
        public Guid Id { get; set; }
        public string TakipNo { get; set; } = string.Empty;
        public Guid? OncekiKayitId { get; set; }
        public string IslemTipi { get; set; } = string.Empty;
        public DateTime IslemTarihi { get; set; }
        public string LisansKodu { get; set; } = string.Empty;
        public string AktivasyonKodu { get; set; } = string.Empty;
        public string FirmaAdi { get; set; } = string.Empty;
        public string YetkiliKisi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string LisansTipi { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int MaxKullaniciSayisi { get; set; }
        public int MaxAracSayisi { get; set; }
        public string MakineKodu { get; set; } = string.Empty;
        public bool Aktif { get; set; }
        public string? Notlar { get; set; }
    }

    private sealed class LisansTakipListeSatiri
    {
        public Guid Id { get; set; }
        public string TakipNo { get; set; } = string.Empty;
        public string FirmaAdi { get; set; } = string.Empty;
        public string IslemTipi { get; set; } = string.Empty;
        public string LisansTipi { get; set; } = string.Empty;
        public int MaxKullanici { get; set; }
        public string Baslangic { get; set; } = string.Empty;
        public string Bitis { get; set; } = string.Empty;
        public int KalanGun { get; set; }
        public string Aktif { get; set; } = string.Empty;
    }
}

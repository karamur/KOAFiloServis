using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace CRMFiloServis.LisansDesktop
{
    public partial class Form1 : Form
    {
        private const string LisansAnahtar = "CRMFiloServis2026SecretKey!@#";
        private LisansBilgi? mevcutLisans;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOlustur_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtFirmaAdi.Text))
            {
                MessageBox.Show("Firma adý boþ olamaz!", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirmaAdi.Focus();
                return;
            }

            // Makine kodu kontrolü
            if (string.IsNullOrWhiteSpace(txtMusteriMakineKodu.Text))
            {
                MessageBox.Show("Müþteri makine kodu boþ olamaz!\n\nMüþteri, programda 'Makine Kodunu Al' butonuna týklayarak kendi makine kodunu size göndermelidir.", 
                    "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMusteriMakineKodu.Focus();
                return;
            }

            // Makine kodu formatý kontrolü (en az 16 karakter)
            if (txtMusteriMakineKodu.Text.Trim().Length < 16)
            {
                MessageBox.Show("Geçersiz makine kodu formatý!\n\nMakine kodu en az 16 karakter olmalýdýr.", 
                    "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMusteriMakineKodu.Focus();
                return;
            }

            try
            {
                // Lisans Tipi - Sadece isim için
                string lisansTip;
                
                if (radioTrial.Checked)
                {
                    lisansTip = "Trial";
                }
                else if (radioStandard.Checked)
                {
                    lisansTip = "Standard";
                }
                else if (radioProfessional.Checked)
                {
                    lisansTip = "Professional";
                }
                else
                {
                    lisansTip = "Enterprise";
                }

                // Süreyi kullanýcýdan al
                int toplamGun = (int)numGun.Value;
                int toplamAy = (int)numAy.Value;
                int toplamYil = (int)numYil.Value;

                // En az bir süre girilmiþ mi?
                if (toplamGun == 0 && toplamAy == 0 && toplamYil == 0)
                {
                    MessageBox.Show("Lütfen en az bir süre deðeri girin (Gün, Ay veya Yýl)!", 
                        "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numYil.Focus();
                    return;
                }

                // Bitiþ tarihini hesapla
                var bitisTarihi = DateTime.Today
                    .AddYears(toplamYil)
                    .AddMonths(toplamAy)
                    .AddDays(toplamGun);

                var toplamSure = (bitisTarihi - DateTime.Today).Days;

                // Lisans Oluþtur
                var lisans = new LisansBilgi
                {
                    LisansKodu = GenerateLisansKodu(),
                    FirmaAdi = txtFirmaAdi.Text.Trim(),
                    YetkiliKisi = txtYetkili.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Telefon = txtTelefon.Text.Trim(),
                    LisansTipi = lisansTip,
                    BaslangicTarihi = DateTime.Today,
                    BitisTarihi = bitisTarihi,
                    MaxKullaniciSayisi = (int)numMaxKullanici.Value,
                    MaxAracSayisi = (int)numMaxArac.Value,
                    MakineKodu = txtMusteriMakineKodu.Text.Trim(), // Müþteri makine kodu
                    Aktif = true
                };

                // Lisans Anahtarý Oluþtur
                var lisansJson = JsonSerializer.Serialize(lisans);
                var lisansAnahtari = EncryptString(lisansJson);

                // Sonuçlarý Göster
                mevcutLisans = lisans;
                
                txtLisansBilgi.Text = $@"?????????????????????????????????????????????????????????
?           LÝSANS BAÞARIYLA OLUÞTURULDU!              ?
?????????????????????????????????????????????????????????

Lisans Kodu       : {lisans.LisansKodu}
Firma Adý         : {lisans.FirmaAdi}
Yetkili Kiþi      : {lisans.YetkiliKisi}
Email             : {lisans.Email}
Telefon           : {lisans.Telefon}
Lisans Tipi       : {lisans.LisansTipi}
Baþlangýç Tarihi  : {lisans.BaslangicTarihi:dd.MM.yyyy}
Bitiþ Tarihi      : {lisans.BitisTarihi:dd.MM.yyyy}
Lisans Süresi     : {toplamYil} yýl, {toplamAy} ay, {toplamGun} gün
Toplam Gün        : {toplamSure} gün
Max Kullanýcý     : {lisans.MaxKullaniciSayisi}
Max Araç          : {lisans.MaxAracSayisi}
Makine Kodu       : {FormatMachineCode(lisans.MakineKodu)}

??  ÖNEMLÝ: Bu lisans SADECE belirtilen makine koduna sahip 
             bilgisayarda geçerlidir!

???????????????????????????????????????????????????????
LÝSANS ANAHTARI (Aþaðýda):
";
                
                txtLisansAnahtari.Text = lisansAnahtari;

                btnKopyala.Enabled = true;
                btnKaydet.Enabled = true;

                MessageBox.Show("Lisans baþarýyla oluþturuldu!\n\nLütfen lisans anahtarýný kopyalayýn veya dosyaya kaydedin.",
                    "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMakineKoduAl_Click(object sender, EventArgs e)
        {
            try
            {
                // Bu PC'nin makine kodunu al (test amaçlý)
                var machineCode = GetMachineCode();
                txtMusteriMakineKodu.Text = machineCode;
                
                MessageBox.Show($"Bu bilgisayarýn makine kodu alýndý:\n\n{FormatMachineCode(machineCode)}\n\n?? NOT: Gerçek lisans oluþturmada MÜÞTERÝNÝN makine kodunu kullanmalýsýnýz!\n\nMüþteri, programda 'Lisans Bilgileri' sayfasýndaki makine kodunu size göndermelidir.", 
                    "Makine Kodu", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine kodu alýnamadý:\n{ex.Message}", 
                    "Hata", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void btnKopyala_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLisansAnahtari.Text))
            {
                Clipboard.SetText(txtLisansAnahtari.Text);
                MessageBox.Show("Lisans anahtarý panoya kopyalandý!", "Baþarýlý", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (mevcutLisans == null) return;

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text Dosyasý|*.txt",
                    FileName = $"Lisans_{mevcutLisans.LisansKodu}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    Title = "Lisans Dosyasýný Kaydet"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var content = $@"CRM FÝLO SERVÝS LÝSANS BÝLGÝSÝ
???????????????????????????????????????

Lisans Kodu       : {mevcutLisans.LisansKodu}
Firma Adý         : {mevcutLisans.FirmaAdi}
Yetkili Kiþi      : {mevcutLisans.YetkiliKisi}
Email             : {mevcutLisans.Email}
Telefon           : {mevcutLisans.Telefon}
Lisans Tipi       : {mevcutLisans.LisansTipi}
Baþlangýç Tarihi  : {mevcutLisans.BaslangicTarihi:dd.MM.yyyy}
Bitiþ Tarihi      : {mevcutLisans.BitisTarihi:dd.MM.yyyy}
Max Kullanýcý     : {mevcutLisans.MaxKullaniciSayisi}
Max Araç          : {mevcutLisans.MaxAracSayisi}

LÝSANS ANAHTARI:
???????????????????????????????????????
{txtLisansAnahtari.Text}

NOT: Bu anahtarý güvenli bir yerde saklayýn!
Programda Ayarlar > Lisans menüsünden girebilirsiniz.
";

                    File.WriteAllText(saveDialog.FileName, content, Encoding.UTF8);
                    MessageBox.Show($"Lisans dosyasý baþarýyla kaydedildi:\n{saveDialog.FileName}",
                        "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDogrula_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDogrulaAnahtar.Text))
            {
                MessageBox.Show("Lütfen lisans anahtarýný girin!", "Uyarý", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var lisansJson = DecryptString(txtDogrulaAnahtar.Text.Trim());
                var lisans = JsonSerializer.Deserialize<LisansBilgi>(lisansJson);

                if (lisans == null)
                {
                    txtDogrulaSonuc.Text = "? GEÇERSÝZ LÝSANS ANAHTARI!";
                    txtDogrulaSonuc.ForeColor = Color.Red;
                    MessageBox.Show("Geçersiz lisans anahtarý!", "Hata", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var kalanGun = (lisans.BitisTarihi - DateTime.Today).Days;
                var gecerli = lisans.Aktif && kalanGun >= 0;

                txtDogrulaSonuc.ForeColor = gecerli ? Color.Green : Color.Red;

                txtDogrulaSonuc.Text = $@"?????????????????????????????????????????????????????????
?     {(gecerli ? "? LÝSANS GEÇERLÝ" : "? LÝSANS GEÇERSÝZ")}                            ?
?????????????????????????????????????????????????????????

Lisans Kodu       : {lisans.LisansKodu}
Firma Adý         : {lisans.FirmaAdi}
Yetkili Kiþi      : {lisans.YetkiliKisi}
Email             : {lisans.Email}
Telefon           : {lisans.Telefon}
Lisans Tipi       : {lisans.LisansTipi}
Baþlangýç Tarihi  : {lisans.BaslangicTarihi:dd.MM.yyyy}
Bitiþ Tarihi      : {lisans.BitisTarihi:dd.MM.yyyy}
{(gecerli ? $"Kalan Gün         : {kalanGun} gün" : $"Lisans Süresi     : {Math.Abs(kalanGun)} gün önce dolmuþ!")}
Max Kullanýcý     : {lisans.MaxKullaniciSayisi}
Max Araç          : {lisans.MaxAracSayisi}
Durum             : {(lisans.Aktif ? "Aktif" : "Pasif")}
";

                MessageBox.Show(gecerli ? "Lisans geçerli!" : "Lisans süresi dolmuþ veya geçersiz!",
                    gecerli ? "Baþarýlý" : "Uyarý",
                    MessageBoxButtons.OK,
                    gecerli ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                txtDogrulaSonuc.Text = $"? HATA:\n{ex.Message}";
                txtDogrulaSonuc.ForeColor = Color.Red;
                MessageBox.Show($"Lisans doðrulama hatasý:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GenerateLisansKodu()
        {
            var random = new Random();
            var part1 = random.Next(1000, 9999);
            var part2 = random.Next(1000, 9999);
            var part3 = random.Next(1000, 9999);
            var part4 = random.Next(1000, 9999);
            return $"CRM-{part1}-{part2}-{part3}-{part4}";
        }

        private static string EncryptString(string plainText)
        {
            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(LisansAnahtar));
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);

            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        private static string DecryptString(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(LisansAnahtar));
            aes.Key = key;

            var iv = new byte[aes.IV.Length];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }

        private string FormatMachineCode(string code)
        {
            // Makine kodunu belirli bir formatta göster
            if (string.IsNullOrEmpty(code) || code.Length < 4)
                return code;
                
            return string.Join("-", Enumerable.Range(0, code.Length / 4)
                .Select(i => code.Substring(i * 4, Math.Min(4, code.Length - i * 4))));
        }

        private static string GetMachineCode()
        {
            try
            {
                // Windows için System.Management kullan
                if (OperatingSystem.IsWindows())
                {
                    var cpuId = GetCpuId();
                    var mbSerial = GetMotherboardSerial();
                    var diskSerial = GetDiskSerial();
                    
                    var combined = $"{cpuId}-{mbSerial}-{diskSerial}";
                    using var sha = SHA256.Create();
                    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return Convert.ToBase64String(hash).Substring(0, 32).Replace("/", "").Replace("+", "");
                }
                
                // Diðer platformlar için basit kod
                var machineName = Environment.MachineName;
                var userName = Environment.UserName;
                var osVersion = Environment.OSVersion.ToString();
                
                var fallbackCombined = $"{machineName}-{userName}-{osVersion}";
                using var fallbackSha = SHA256.Create();
                var fallbackHash = fallbackSha.ComputeHash(Encoding.UTF8.GetBytes(fallbackCombined));
                return Convert.ToBase64String(fallbackHash).Substring(0, 32).Replace("/", "").Replace("+", "");
            }
            catch
            {
                return "LOCAL-DEV-MODE-00000000";
            }
        }

        private static string GetCpuId()
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return "CPU-UNKNOWN";
                
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? "CPU-UNKNOWN";
                }
            }
            catch { }
            return "CPU-UNKNOWN";
        }

        private static string GetMotherboardSerial()
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return "MB-UNKNOWN";
                
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var obj in searcher.Get())
                {
                    return obj["SerialNumber"]?.ToString() ?? "MB-UNKNOWN";
                }
            }
            catch { }
            return "MB-UNKNOWN";
        }

        private static string GetDiskSerial()
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return "DISK-UNKNOWN";
                
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
                foreach (var obj in searcher.Get())
                {
                    var serial = obj["SerialNumber"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(serial))
                        return serial;
                }
            }
            catch { }
            return "DISK-UNKNOWN";
        }
    }

    public class LisansBilgi
    {
        public string LisansKodu { get; set; } = "";
        public string FirmaAdi { get; set; } = "";
        public string YetkiliKisi { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string LisansTipi { get; set; } = "";
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int MaxKullaniciSayisi { get; set; }
        public int MaxAracSayisi { get; set; }
        public string MakineKodu { get; set; } = ""; // Müþteri makine kodu
        public bool Aktif { get; set; }
    }
}

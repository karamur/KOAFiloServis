using System;
using System.Windows.Forms;

namespace CRMFiloServis.LisansDesktop
{
    public class LisansYenileForm : Form
    {
        private readonly LisansKayit _mevcutLisans;

        public string YeniLisansTipi { get; private set; } = "";
        public DateTime YeniBaslangic { get; private set; }
        public DateTime YeniBitis { get; private set; }
        public int YeniMaxKullanici { get; private set; }
        public int YeniMaxArac { get; private set; }

        private ComboBox cmbLisansTipi;
        private DateTimePicker dtpBaslangic;
        private NumericUpDown numYil;
        private NumericUpDown numAy;
        private NumericUpDown numGun;
        private NumericUpDown numMaxKullanici;
        private NumericUpDown numMaxArac;
        private Label lblMevcutBilgi;
        private Button btnYenile;
        private Button btnIptal;

        public LisansYenileForm(LisansKayit mevcutLisans)
        {
            _mevcutLisans = mevcutLisans;
            InitializeComponent();
            LoadMevcutBilgiler();
        }

        private void InitializeComponent()
        {
            this.Text = "Lisans Yenileme";
            this.Size = new System.Drawing.Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var y = 20;

            // Mevcut Bilgi
            lblMevcutBilgi = new Label
            {
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(400, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.Color.LightYellow
            };
            this.Controls.Add(lblMevcutBilgi);
            y += 100;

            // Lisans Tipi
            var lblTip = new Label { Text = "Yeni Lisans Tipi:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            this.Controls.Add(lblTip);
            y += 25;

            cmbLisansTipi = new ComboBox
            {
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLisansTipi.Items.AddRange(new[] { "Trial", "Standard", "Professional", "Enterprise" });
            this.Controls.Add(cmbLisansTipi);
            y += 40;

            // Baþlangýç Tarihi
            var lblBaslangic = new Label { Text = "Yeni Baþlangýç Tarihi:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            this.Controls.Add(lblBaslangic);
            y += 25;

            dtpBaslangic = new DateTimePicker
            {
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(200, 25),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpBaslangic);
            y += 40;

            // Süre
            var lblSure = new Label { Text = "Lisans Süresi:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            this.Controls.Add(lblSure);
            y += 25;

            var lblYil = new Label { Text = "Yýl:", Location = new System.Drawing.Point(20, y + 3), AutoSize = true };
            this.Controls.Add(lblYil);
            numYil = new NumericUpDown { Location = new System.Drawing.Point(50, y), Size = new System.Drawing.Size(60, 25), Maximum = 10, Value = 1 };
            this.Controls.Add(numYil);

            var lblAy = new Label { Text = "Ay:", Location = new System.Drawing.Point(130, y + 3), AutoSize = true };
            this.Controls.Add(lblAy);
            numAy = new NumericUpDown { Location = new System.Drawing.Point(160, y), Size = new System.Drawing.Size(60, 25), Maximum = 11, Value = 0 };
            this.Controls.Add(numAy);

            var lblGun = new Label { Text = "Gün:", Location = new System.Drawing.Point(240, y + 3), AutoSize = true };
            this.Controls.Add(lblGun);
            numGun = new NumericUpDown { Location = new System.Drawing.Point(280, y), Size = new System.Drawing.Size(60, 25), Maximum = 30, Value = 0 };
            this.Controls.Add(numGun);
            y += 40;

            // Max Kullanýcý
            var lblKullanici = new Label { Text = "Max Kullanýcý:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            this.Controls.Add(lblKullanici);
            numMaxKullanici = new NumericUpDown { Location = new System.Drawing.Point(120, y - 3), Size = new System.Drawing.Size(80, 25), Maximum = 1000, Minimum = 1, Value = 10 };
            this.Controls.Add(numMaxKullanici);
            y += 35;

            // Max Araç
            var lblArac = new Label { Text = "Max Araç:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            this.Controls.Add(lblArac);
            numMaxArac = new NumericUpDown { Location = new System.Drawing.Point(120, y - 3), Size = new System.Drawing.Size(80, 25), Maximum = 10000, Minimum = 1, Value = 100 };
            this.Controls.Add(numMaxArac);
            y += 50;

            // Butonlar
            btnYenile = new Button
            {
                Text = "Lisansý Yenile",
                Location = new System.Drawing.Point(100, y),
                Size = new System.Drawing.Size(120, 35),
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White
            };
            btnYenile.Click += BtnYenile_Click;
            this.Controls.Add(btnYenile);

            btnIptal = new Button
            {
                Text = "Ýptal",
                Location = new System.Drawing.Point(230, y),
                Size = new System.Drawing.Size(100, 35)
            };
            btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnIptal);
        }

        private void LoadMevcutBilgiler()
        {
            var kalanGun = (_mevcutLisans.BitisTarihi - DateTime.Today).Days;
            lblMevcutBilgi.Text = $@"MEVCUT LÝSANS BÝLGÝSÝ:
Firma: {_mevcutLisans.FirmaAdi}
Lisans Tipi: {_mevcutLisans.LisansTipi}
Bitiþ: {_mevcutLisans.BitisTarihi:dd.MM.yyyy} ({(kalanGun >= 0 ? $"{kalanGun} gün kaldý" : "Süresi dolmuþ!")})";

            // Mevcut deðerleri yükle
            var tipIndex = cmbLisansTipi.Items.IndexOf(_mevcutLisans.LisansTipi);
            cmbLisansTipi.SelectedIndex = tipIndex >= 0 ? tipIndex : 1;

            dtpBaslangic.Value = DateTime.Today;
            numYil.Value = 1;
            numMaxKullanici.Value = _mevcutLisans.MaxKullaniciSayisi;
            numMaxArac.Value = _mevcutLisans.MaxAracSayisi;
        }

        private void BtnYenile_Click(object? sender, EventArgs e)
        {
            var toplamGun = (int)numGun.Value;
            var toplamAy = (int)numAy.Value;
            var toplamYil = (int)numYil.Value;

            if (toplamGun == 0 && toplamAy == 0 && toplamYil == 0)
            {
                MessageBox.Show("Lütfen en az bir süre deðeri girin!", "Uyarý", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            YeniLisansTipi = cmbLisansTipi.SelectedItem?.ToString() ?? "Standard";
            YeniBaslangic = dtpBaslangic.Value.Date;
            YeniBitis = YeniBaslangic.AddYears(toplamYil).AddMonths(toplamAy).AddDays(toplamGun);
            YeniMaxKullanici = (int)numMaxKullanici.Value;
            YeniMaxArac = (int)numMaxArac.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

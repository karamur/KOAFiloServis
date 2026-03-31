namespace CRMFiloServis.Installer;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "CRM Filo Servis Kurulum";
        this.Size = new System.Drawing.Size(700, 680);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        // Başlık
        var lblBaslik = new Label
        {
            Text = "CRM Filo Servis Kurulum Sihirbazı",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 15),
            AutoSize = true
        };
        this.Controls.Add(lblBaslik);

        // Kurulum Tipi Grubu
        var grpTip = new GroupBox
        {
            Text = "1. Kurulum Tipi",
            Location = new Point(20, 55),
            Size = new Size(640, 70)
        };
        this.Controls.Add(grpTip);

        rbNormal = new RadioButton
        {
            Text = "Normal Kurulum (IIS / Windows Server)",
            Location = new Point(20, 28),
            AutoSize = true,
            Checked = true
        };
        rbNormal.CheckedChanged += rbNormal_CheckedChanged;
        grpTip.Controls.Add(rbNormal);

        rbDocker = new RadioButton
        {
            Text = "Docker Kurulum",
            Location = new Point(320, 28),
            AutoSize = true
        };
        rbDocker.CheckedChanged += rbDocker_CheckedChanged;
        grpTip.Controls.Add(rbDocker);

        // Kurulum Modu Grubu
        var grpMod = new GroupBox
        {
            Text = "2. Kurulum Modu",
            Location = new Point(20, 135),
            Size = new Size(640, 70)
        };
        this.Controls.Add(grpMod);

        rbYeniKurulum = new RadioButton
        {
            Text = "Sıfır Kurulum (Yeni veritabanı)",
            Location = new Point(20, 28),
            AutoSize = true,
            Checked = true
        };
        rbYeniKurulum.CheckedChanged += rbYeniKurulum_CheckedChanged;
        grpMod.Controls.Add(rbYeniKurulum);

        rbMevcutYedek = new RadioButton
        {
            Text = "Mevcut Yedek İle Kurulum",
            Location = new Point(320, 28),
            AutoSize = true
        };
        rbMevcutYedek.CheckedChanged += rbMevcutYedek_CheckedChanged;
        grpMod.Controls.Add(rbMevcutYedek);

        // Yedek Grubu
        grpYedek = new GroupBox
        {
            Text = "3. Veritabanı Yedeği",
            Location = new Point(20, 215),
            Size = new Size(640, 70),
            Enabled = false
        };
        this.Controls.Add(grpYedek);

        lblYedekDurum = new Label
        {
            Text = "Sıfır veritabanı ile kurulum yapılacak.",
            Location = new Point(20, 28),
            AutoSize = true
        };
        grpYedek.Controls.Add(lblYedekDurum);

        var btnYedekSec = new Button
        {
            Text = "Yedek Dosyası Seç...",
            Location = new Point(480, 24),
            Size = new Size(140, 28)
        };
        btnYedekSec.Click += btnYedekSec_Click;
        grpYedek.Controls.Add(btnYedekSec);

        // Paket Grubu
        var grpPaket = new GroupBox
        {
            Text = "4. Kurulum Paketi",
            Location = new Point(20, 295),
            Size = new Size(640, 70)
        };
        this.Controls.Add(grpPaket);

        lblPaketDurum = new Label
        {
            Text = "Kurulum paketi seçilmedi.",
            Location = new Point(20, 28),
            AutoSize = true
        };
        grpPaket.Controls.Add(lblPaketDurum);

        var btnPaketSec = new Button
        {
            Text = "Paket Seç...",
            Location = new Point(480, 24),
            Size = new Size(140, 28)
        };
        btnPaketSec.Click += btnPaketSec_Click;
        grpPaket.Controls.Add(btnPaketSec);

        // Hedef Dizin Grubu
        var grpDizin = new GroupBox
        {
            Text = "5. Hedef Dizin",
            Location = new Point(20, 375),
            Size = new Size(640, 70)
        };
        this.Controls.Add(grpDizin);

        txtHedefDizin = new TextBox
        {
            Text = @"C:\CRMFiloServis",
            Location = new Point(20, 28),
            Size = new Size(430, 23),
            ReadOnly = true
        };
        grpDizin.Controls.Add(txtHedefDizin);

        var btnDizinSec = new Button
        {
            Text = "Dizin Seç...",
            Location = new Point(480, 24),
            Size = new Size(140, 28)
        };
        btnDizinSec.Click += btnDizinSec_Click;
        grpDizin.Controls.Add(btnDizinSec);

        // Normal Ayarlar Paneli
        pnlNormalAyarlari = new Panel
        {
            Location = new Point(20, 455),
            Size = new Size(640, 50),
            Visible = true
        };
        this.Controls.Add(pnlNormalAyarlari);

        var lblNormalInfo = new Label
        {
            Text = "Not: IIS ve .NET 10 Hosting Bundle kurulu olmalıdır.",
            Location = new Point(0, 15),
            AutoSize = true,
            ForeColor = Color.DarkBlue
        };
        pnlNormalAyarlari.Controls.Add(lblNormalInfo);

        // Docker Ayarlar Paneli
        pnlDockerAyarlari = new Panel
        {
            Location = new Point(20, 455),
            Size = new Size(640, 50),
            Visible = false
        };
        this.Controls.Add(pnlDockerAyarlari);

        var lblDockerInfo = new Label
        {
            Text = "Not: Docker Desktop kurulu olmalıdır. Compose dosyaları oluşturulacak.",
            Location = new Point(0, 15),
            AutoSize = true,
            ForeColor = Color.DarkGreen
        };
        pnlDockerAyarlari.Controls.Add(lblDockerInfo);

        // Makine Kodu Grubu
        var grpMakineKodu = new GroupBox
        {
            Text = "Lisans için Makine Kodu",
            Location = new Point(20, 510),
            Size = new Size(640, 60)
        };
        this.Controls.Add(grpMakineKodu);

        txtMakineKodu = new TextBox
        {
            Location = new Point(20, 24),
            Size = new Size(430, 23),
            ReadOnly = true,
            Font = new Font("Consolas", 9)
        };
        grpMakineKodu.Controls.Add(txtMakineKodu);

        var btnMakineKoduKopyala = new Button
        {
            Text = "Kopyala",
            Location = new Point(480, 22),
            Size = new Size(140, 28)
        };
        btnMakineKoduKopyala.Click += btnMakineKoduKopyala_Click;
        grpMakineKodu.Controls.Add(btnMakineKoduKopyala);

        // Progress ve Durum
        progressBar = new ProgressBar
        {
            Location = new Point(20, 580),
            Size = new Size(540, 25),
            Visible = false
        };
        this.Controls.Add(progressBar);

        lblDurum = new Label
        {
            Text = "Hazır",
            Location = new Point(20, 610),
            AutoSize = true,
            Visible = false
        };
        this.Controls.Add(lblDurum);

        // Başlat Butonu
        btnKurulumBaslat = new Button
        {
            Text = "Kurulumu Başlat",
            Location = new Point(480, 580),
            Size = new Size(180, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.ForestGreen,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnKurulumBaslat.Click += btnKurulumBaslat_Click;
        this.Controls.Add(btnKurulumBaslat);
    }

    private RadioButton rbNormal;
    private RadioButton rbDocker;
    private RadioButton rbYeniKurulum;
    private RadioButton rbMevcutYedek;
    private GroupBox grpYedek;
    private Label lblYedekDurum;
    private Label lblPaketDurum;
    private TextBox txtHedefDizin;
    private TextBox txtMakineKodu;
    private Panel pnlNormalAyarlari;
    private Panel pnlDockerAyarlari;
    private ProgressBar progressBar;
    private Label lblDurum;
    private Button btnKurulumBaslat;
}

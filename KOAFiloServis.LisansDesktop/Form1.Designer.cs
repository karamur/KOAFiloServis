namespace KOAFiloServis.LisansDesktop;

partial class Form1
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblBaslik = new Label();
        lblFirmaAdi = new Label();
        txtFirmaAdi = new TextBox();
        lblYetkili = new Label();
        txtYetkili = new TextBox();
        lblEmail = new Label();
        txtEmail = new TextBox();
        lblTelefon = new Label();
        txtTelefon = new TextBox();
        lblMakineKodu = new Label();
        txtMakineKodu = new TextBox();
        btnBuPcMakineKodu = new Button();
        lblLisansTipi = new Label();
        cmbLisansTipi = new ComboBox();
        lblMaxKullanici = new Label();
        numMaxKullanici = new NumericUpDown();
        lblBitisTarihi = new Label();
        dtpBitisTarihi = new DateTimePicker();
        btnLisansUret = new Button();
        btnKopyala = new Button();
        btnTxtKaydet = new Button();
        txtLisansAnahtari = new TextBox();
        lblAnahtar = new Label();
        saveFileDialog1 = new SaveFileDialog();
        lblIslemTipi = new Label();
        cmbIslemTipi = new ComboBox();
        lblNotlar = new Label();
        txtNotlar = new TextBox();
        dgvKayitlar = new DataGridView();
        btnSeciliKaydiYukle = new Button();
        btnYeniForm = new Button();
        btnPaketOlustur = new Button();
        lblKayitlar = new Label();
        ((System.ComponentModel.ISupportInitialize)numMaxKullanici).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvKayitlar).BeginInit();
        SuspendLayout();
        // 
        // lblBaslik
        // 
        lblBaslik.AutoSize = true;
        lblBaslik.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblBaslik.Location = new Point(24, 18);
        lblBaslik.Name = "lblBaslik";
        lblBaslik.Size = new Size(296, 35);
        lblBaslik.TabIndex = 0;
        lblBaslik.Text = "Koa Filo Servis Lisans Oluşturucu";
        // 
        // lblFirmaAdi
        // 
        lblFirmaAdi.AutoSize = true;
        lblFirmaAdi.Location = new Point(28, 79);
        lblFirmaAdi.Name = "lblFirmaAdi";
        lblFirmaAdi.Size = new Size(69, 20);
        lblFirmaAdi.TabIndex = 1;
        lblFirmaAdi.Text = "Firma Adı";
        // 
        // txtFirmaAdi
        // 
        txtFirmaAdi.Location = new Point(167, 76);
        txtFirmaAdi.Name = "txtFirmaAdi";
        txtFirmaAdi.Size = new Size(350, 27);
        txtFirmaAdi.TabIndex = 0;
        // 
        // lblYetkili
        // 
        lblYetkili.AutoSize = true;
        lblYetkili.Location = new Point(28, 117);
        lblYetkili.Name = "lblYetkili";
        lblYetkili.Size = new Size(86, 20);
        lblYetkili.TabIndex = 3;
        lblYetkili.Text = "Yetkili Kişi";
        // 
        // txtYetkili
        // 
        txtYetkili.Location = new Point(167, 114);
        txtYetkili.Name = "txtYetkili";
        txtYetkili.Size = new Size(350, 27);
        txtYetkili.TabIndex = 1;
        // 
        // lblEmail
        // 
        lblEmail.AutoSize = true;
        lblEmail.Location = new Point(28, 155);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(46, 20);
        lblEmail.TabIndex = 5;
        lblEmail.Text = "Email";
        // 
        // txtEmail
        // 
        txtEmail.Location = new Point(167, 152);
        txtEmail.Name = "txtEmail";
        txtEmail.Size = new Size(350, 27);
        txtEmail.TabIndex = 2;
        // 
        // lblTelefon
        // 
        lblTelefon.AutoSize = true;
        lblTelefon.Location = new Point(28, 193);
        lblTelefon.Name = "lblTelefon";
        lblTelefon.Size = new Size(56, 20);
        lblTelefon.TabIndex = 7;
        lblTelefon.Text = "Telefon";
        // 
        // txtTelefon
        // 
        txtTelefon.Location = new Point(167, 190);
        txtTelefon.Name = "txtTelefon";
        txtTelefon.Size = new Size(350, 27);
        txtTelefon.TabIndex = 3;
        // 
        // lblMakineKodu
        // 
        lblMakineKodu.AutoSize = true;
        lblMakineKodu.Location = new Point(28, 231);
        lblMakineKodu.Name = "lblMakineKodu";
        lblMakineKodu.Size = new Size(93, 20);
        lblMakineKodu.TabIndex = 9;
        lblMakineKodu.Text = "Makine Kodu";
        // 
        // txtMakineKodu
        // 
        txtMakineKodu.Font = new Font("Consolas", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 162);
        txtMakineKodu.Location = new Point(167, 228);
        txtMakineKodu.Name = "txtMakineKodu";
        txtMakineKodu.Size = new Size(350, 27);
        txtMakineKodu.TabIndex = 4;
        // 
        // btnBuPcMakineKodu
        // 
        btnBuPcMakineKodu.Location = new Point(523, 226);
        btnBuPcMakineKodu.Name = "btnBuPcMakineKodu";
        btnBuPcMakineKodu.Size = new Size(120, 31);
        btnBuPcMakineKodu.TabIndex = 5;
        btnBuPcMakineKodu.Text = "Bu PC Kodu";
        btnBuPcMakineKodu.UseVisualStyleBackColor = true;
        btnBuPcMakineKodu.Click += btnBuPcMakineKodu_Click;
        // 
        // lblLisansTipi
        // 
        lblLisansTipi.AutoSize = true;
        lblLisansTipi.Location = new Point(28, 269);
        lblLisansTipi.Name = "lblLisansTipi";
        lblLisansTipi.Size = new Size(78, 20);
        lblLisansTipi.TabIndex = 12;
        lblLisansTipi.Text = "Lisans Tipi";
        // 
        // cmbLisansTipi
        // 
        cmbLisansTipi.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLisansTipi.FormattingEnabled = true;
        cmbLisansTipi.Items.AddRange(new object[] { "Trial", "Standard", "Professional", "Enterprise" });
        cmbLisansTipi.Location = new Point(167, 266);
        cmbLisansTipi.Name = "cmbLisansTipi";
        cmbLisansTipi.Size = new Size(162, 28);
        cmbLisansTipi.TabIndex = 6;
        // 
        // lblMaxKullanici
        // 
        lblMaxKullanici.AutoSize = true;
        lblMaxKullanici.Location = new Point(350, 269);
        lblMaxKullanici.Name = "lblMaxKullanici";
        lblMaxKullanici.Size = new Size(97, 20);
        lblMaxKullanici.TabIndex = 14;
        lblMaxKullanici.Text = "Max Kullanıcı";
        // 
        // numMaxKullanici
        // 
        numMaxKullanici.Location = new Point(453, 267);
        numMaxKullanici.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        numMaxKullanici.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numMaxKullanici.Name = "numMaxKullanici";
        numMaxKullanici.Size = new Size(90, 27);
        numMaxKullanici.TabIndex = 7;
        numMaxKullanici.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // lblBitisTarihi
        // 
        lblBitisTarihi.AutoSize = true;
        lblBitisTarihi.Location = new Point(28, 307);
        lblBitisTarihi.Name = "lblBitisTarihi";
        lblBitisTarihi.Size = new Size(77, 20);
        lblBitisTarihi.TabIndex = 16;
        lblBitisTarihi.Text = "Bitiş Tarihi";
        // 
        // dtpBitisTarihi
        // 
        dtpBitisTarihi.Location = new Point(167, 304);
        dtpBitisTarihi.Name = "dtpBitisTarihi";
        dtpBitisTarihi.Size = new Size(376, 27);
        dtpBitisTarihi.TabIndex = 8;
        // 
        // lblIslemTipi
        // 
        lblIslemTipi.AutoSize = true;
        lblIslemTipi.Location = new Point(28, 345);
        lblIslemTipi.Name = "lblIslemTipi";
        lblIslemTipi.Size = new Size(76, 20);
        lblIslemTipi.TabIndex = 22;
        lblIslemTipi.Text = "İşlem Tipi";
        // 
        // cmbIslemTipi
        // 
        cmbIslemTipi.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbIslemTipi.FormattingEnabled = true;
        cmbIslemTipi.Items.AddRange(new object[] { "Yeni Kayıt", "Yenileme", "Güncelleme" });
        cmbIslemTipi.Location = new Point(167, 342);
        cmbIslemTipi.Name = "cmbIslemTipi";
        cmbIslemTipi.Size = new Size(162, 28);
        cmbIslemTipi.TabIndex = 9;
        // 
        // lblNotlar
        // 
        lblNotlar.AutoSize = true;
        lblNotlar.Location = new Point(28, 383);
        lblNotlar.Name = "lblNotlar";
        lblNotlar.Size = new Size(51, 20);
        lblNotlar.TabIndex = 24;
        lblNotlar.Text = "Notlar";
        // 
        // txtNotlar
        // 
        txtNotlar.Location = new Point(167, 380);
        txtNotlar.Multiline = true;
        txtNotlar.Name = "txtNotlar";
        txtNotlar.ScrollBars = ScrollBars.Vertical;
        txtNotlar.Size = new Size(476, 72);
        txtNotlar.TabIndex = 10;
        // 
        // btnLisansUret
        // 
        btnLisansUret.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
        btnLisansUret.Location = new Point(167, 468);
        btnLisansUret.Name = "btnLisansUret";
        btnLisansUret.Size = new Size(180, 41);
        btnLisansUret.TabIndex = 11;
        btnLisansUret.Text = "Lisans Anahtarı Üret";
        btnLisansUret.UseVisualStyleBackColor = true;
        btnLisansUret.Click += btnLisansUret_Click;
        // 
        // btnKopyala
        // 
        btnKopyala.Location = new Point(361, 468);
        btnKopyala.Name = "btnKopyala";
        btnKopyala.Size = new Size(135, 41);
        btnKopyala.TabIndex = 12;
        btnKopyala.Text = "Panoya Kopyala";
        btnKopyala.UseVisualStyleBackColor = true;
        btnKopyala.Click += btnKopyala_Click;
        // 
        // btnTxtKaydet
        // 
        btnTxtKaydet.Location = new Point(508, 468);
        btnTxtKaydet.Name = "btnTxtKaydet";
        btnTxtKaydet.Size = new Size(135, 41);
        btnTxtKaydet.TabIndex = 13;
        btnTxtKaydet.Text = "Txt Kaydet";
        btnTxtKaydet.UseVisualStyleBackColor = true;
        btnTxtKaydet.Click += btnTxtKaydet_Click;
        // 
        // lblAnahtar
        // 
        lblAnahtar.AutoSize = true;
        lblAnahtar.Location = new Point(28, 525);
        lblAnahtar.Name = "lblAnahtar";
        lblAnahtar.Size = new Size(141, 20);
        lblAnahtar.TabIndex = 21;
        lblAnahtar.Text = "Lisans Aktivasyon Kodu";
        // 
        // txtLisansAnahtari
        // 
        txtLisansAnahtari.Font = new Font("Consolas", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 162);
        txtLisansAnahtari.Location = new Point(28, 551);
        txtLisansAnahtari.Multiline = true;
        txtLisansAnahtari.Name = "txtLisansAnahtari";
        txtLisansAnahtari.ReadOnly = true;
        txtLisansAnahtari.ScrollBars = ScrollBars.Vertical;
        txtLisansAnahtari.Size = new Size(615, 142);
        txtLisansAnahtari.TabIndex = 20;
        // 
        // lblKayitlar
        // 
        lblKayitlar.AutoSize = true;
        lblKayitlar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblKayitlar.Location = new Point(675, 23);
        lblKayitlar.Name = "lblKayitlar";
        lblKayitlar.Size = new Size(142, 28);
        lblKayitlar.TabIndex = 25;
        lblKayitlar.Text = "Lisans Takibi";
        // 
        // dgvKayitlar
        // 
        dgvKayitlar.AllowUserToAddRows = false;
        dgvKayitlar.AllowUserToDeleteRows = false;
        dgvKayitlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvKayitlar.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvKayitlar.Location = new Point(675, 76);
        dgvKayitlar.MultiSelect = false;
        dgvKayitlar.Name = "dgvKayitlar";
        dgvKayitlar.ReadOnly = true;
        dgvKayitlar.RowHeadersWidth = 51;
        dgvKayitlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvKayitlar.Size = new Size(655, 617);
        dgvKayitlar.TabIndex = 26;
        dgvKayitlar.SelectionChanged += dgvKayitlar_SelectionChanged;
        // 
        // btnSeciliKaydiYukle
        // 
        btnSeciliKaydiYukle.Location = new Point(1025, 23);
        btnSeciliKaydiYukle.Name = "btnSeciliKaydiYukle";
        btnSeciliKaydiYukle.Size = new Size(150, 35);
        btnSeciliKaydiYukle.TabIndex = 14;
        btnSeciliKaydiYukle.Text = "Kaydı Forma Yükle";
        btnSeciliKaydiYukle.UseVisualStyleBackColor = true;
        btnSeciliKaydiYukle.Click += btnSeciliKaydiYukle_Click;
        // 
        // btnYeniForm
        // 
        btnYeniForm.Location = new Point(1187, 23);
        btnYeniForm.Name = "btnYeniForm";
        btnYeniForm.Size = new Size(143, 35);
        btnYeniForm.TabIndex = 15;
        btnYeniForm.Text = "Yeni Form Temizle";
        btnYeniForm.UseVisualStyleBackColor = true;
        btnYeniForm.Click += btnYeniForm_Click;
        // 
        // btnPaketOlustur
        // 
        btnPaketOlustur.Location = new Point(880, 18);
        btnPaketOlustur.Name = "btnPaketOlustur";
        btnPaketOlustur.Size = new Size(220, 38);
        btnPaketOlustur.TabIndex = 16;
        btnPaketOlustur.Text = "IIS Paketi Olustur...";
        btnPaketOlustur.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnPaketOlustur.BackColor = Color.LightSteelBlue;
        btnPaketOlustur.UseVisualStyleBackColor = false;
        btnPaketOlustur.Click += btnPaketOlustur_Click;
        // 
        // saveFileDialog1
        // 
        saveFileDialog1.DefaultExt = "txt";
        saveFileDialog1.Filter = "Metin Dosyası|*.txt";
        saveFileDialog1.Title = "Lisans Kodunu Kaydet";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1364, 720);
        Controls.Add(btnYeniForm);
        Controls.Add(btnPaketOlustur);
        Controls.Add(btnSeciliKaydiYukle);
        Controls.Add(dgvKayitlar);
        Controls.Add(lblKayitlar);
        Controls.Add(txtNotlar);
        Controls.Add(lblNotlar);
        Controls.Add(cmbIslemTipi);
        Controls.Add(lblIslemTipi);
        Controls.Add(lblAnahtar);
        Controls.Add(txtLisansAnahtari);
        Controls.Add(btnTxtKaydet);
        Controls.Add(btnKopyala);
        Controls.Add(btnLisansUret);
        Controls.Add(dtpBitisTarihi);
        Controls.Add(lblBitisTarihi);
        Controls.Add(numMaxKullanici);
        Controls.Add(lblMaxKullanici);
        Controls.Add(cmbLisansTipi);
        Controls.Add(lblLisansTipi);
        Controls.Add(btnBuPcMakineKodu);
        Controls.Add(txtMakineKodu);
        Controls.Add(lblMakineKodu);
        Controls.Add(txtTelefon);
        Controls.Add(lblTelefon);
        Controls.Add(txtEmail);
        Controls.Add(lblEmail);
        Controls.Add(txtYetkili);
        Controls.Add(lblYetkili);
        Controls.Add(txtFirmaAdi);
        Controls.Add(lblFirmaAdi);
        Controls.Add(lblBaslik);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Koa Filo Servis Lisans Oluşturucu";
        ((System.ComponentModel.ISupportInitialize)numMaxKullanici).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvKayitlar).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblBaslik;
    private Label lblFirmaAdi;
    private TextBox txtFirmaAdi;
    private Label lblYetkili;
    private TextBox txtYetkili;
    private Label lblEmail;
    private TextBox txtEmail;
    private Label lblTelefon;
    private TextBox txtTelefon;
    private Label lblMakineKodu;
    private TextBox txtMakineKodu;
    private Button btnBuPcMakineKodu;
    private Label lblLisansTipi;
    private ComboBox cmbLisansTipi;
    private Label lblMaxKullanici;
    private NumericUpDown numMaxKullanici;
    private Label lblBitisTarihi;
    private DateTimePicker dtpBitisTarihi;
    private Button btnLisansUret;
    private Button btnKopyala;
    private Button btnTxtKaydet;
    private TextBox txtLisansAnahtari;
    private Label lblAnahtar;
    private SaveFileDialog saveFileDialog1;
    private Label lblIslemTipi;
    private ComboBox cmbIslemTipi;
    private Label lblNotlar;
    private TextBox txtNotlar;
    private DataGridView dgvKayitlar;
    private Button btnSeciliKaydiYukle;
    private Button btnYeniForm;
    private Button btnPaketOlustur;
    private Label lblKayitlar;
}

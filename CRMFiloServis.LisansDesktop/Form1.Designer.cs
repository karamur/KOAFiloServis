namespace CRMFiloServis.LisansDesktop
{
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

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 750);
            this.Text = "CRM Filo Servis - Lisans Yönetimi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // TabControl
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;

            // Tab Sayfalarý
            this.tabYeniLisans = new System.Windows.Forms.TabPage { Text = "Yeni Lisans Oluþtur" };
            this.tabDogrula = new System.Windows.Forms.TabPage { Text = "Lisans Doðrula" };
            this.tabLisansListesi = new System.Windows.Forms.TabPage { Text = "Lisans Listesi & Takip" };

            this.tabControl1.Controls.Add(this.tabYeniLisans);
            this.tabControl1.Controls.Add(this.tabDogrula);
            this.tabControl1.Controls.Add(this.tabLisansListesi);

            // ==================== YENÝ LÝSANS SEKMESÝ ====================
            InitializeYeniLisansTab();

            // ==================== DOÐRULA SEKMESÝ ====================
            InitializeDogrulaTab();

            // ==================== LÝSANS LÝSTESÝ SEKMESÝ ====================
            InitializeLisansListesiTab();

            this.Controls.Add(this.tabControl1);
            this.ResumeLayout(false);
        }

        private void InitializeYeniLisansTab()
        {
            int y = 20;

            // Lisans Tipi GroupBox
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.Text = "Lisans Tipi ve Süre";
            this.groupBox1.Location = new System.Drawing.Point(20, y);
            this.groupBox1.Size = new System.Drawing.Size(500, 280);

            // Radio Buttons
            this.radioTrial = new System.Windows.Forms.RadioButton { Text = "Trial", Location = new System.Drawing.Point(20, 30), AutoSize = true };
            this.radioStandard = new System.Windows.Forms.RadioButton { Text = "Standard", Location = new System.Drawing.Point(20, 55), AutoSize = true, Checked = true };
            this.radioProfessional = new System.Windows.Forms.RadioButton { Text = "Professional", Location = new System.Drawing.Point(20, 80), AutoSize = true };
            this.radioEnterprise = new System.Windows.Forms.RadioButton { Text = "Enterprise", Location = new System.Drawing.Point(20, 105), AutoSize = true };

            this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] { radioTrial, radioStandard, radioProfessional, radioEnterprise });

            // Süre Kontrolleri
            this.lblYil = new System.Windows.Forms.Label { Text = "Yýl:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            this.numYil = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(50, 138), Size = new System.Drawing.Size(60, 23), Maximum = 10, Value = 1 };
            this.lblAy = new System.Windows.Forms.Label { Text = "Ay:", Location = new System.Drawing.Point(130, 140), AutoSize = true };
            this.numAy = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(160, 138), Size = new System.Drawing.Size(60, 23), Maximum = 11 };
            this.lblGun = new System.Windows.Forms.Label { Text = "Gün:", Location = new System.Drawing.Point(240, 140), AutoSize = true };
            this.numGun = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(280, 138), Size = new System.Drawing.Size(60, 23), Maximum = 30 };

            this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] { lblYil, numYil, lblAy, numAy, lblGun, numGun });

            // Makine Kodu
            this.lblMusteriMakineKodu = new System.Windows.Forms.Label { Text = "Müþteri Makine Kodu:", Location = new System.Drawing.Point(20, 180), AutoSize = true };
            this.txtMusteriMakineKodu = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(20, 200), Size = new System.Drawing.Size(350, 23) };
            this.btnMakineKoduAl = new System.Windows.Forms.Button { Text = "Bu PC'nin Kodunu Al", Location = new System.Drawing.Point(380, 198), Size = new System.Drawing.Size(100, 27) };
            this.btnMakineKoduAl.Click += new System.EventHandler(this.btnMakineKoduAl_Click);

            this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] { lblMusteriMakineKodu, txtMusteriMakineKodu, btnMakineKoduAl });

            // Firma Bilgileri GroupBox
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox2.Text = "Firma Bilgileri";
            this.groupBox2.Location = new System.Drawing.Point(540, y);
            this.groupBox2.Size = new System.Drawing.Size(500, 280);

            this.lblFirmaAdi = new System.Windows.Forms.Label { Text = "Firma Adý *:", Location = new System.Drawing.Point(20, 30), AutoSize = true };
            this.txtFirmaAdi = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 27), Size = new System.Drawing.Size(350, 23) };

            this.label1 = new System.Windows.Forms.Label { Text = "Yetkili Kiþi:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            this.txtYetkili = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 57), Size = new System.Drawing.Size(350, 23) };

            this.label2 = new System.Windows.Forms.Label { Text = "E-posta:", Location = new System.Drawing.Point(20, 90), AutoSize = true };
            this.txtEmail = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 87), Size = new System.Drawing.Size(350, 23) };

            this.label3 = new System.Windows.Forms.Label { Text = "Telefon:", Location = new System.Drawing.Point(20, 120), AutoSize = true };
            this.txtTelefon = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 117), Size = new System.Drawing.Size(350, 23) };

            this.label4 = new System.Windows.Forms.Label { Text = "Max Kullanýcý:", Location = new System.Drawing.Point(20, 150), AutoSize = true };
            this.numMaxKullanici = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(130, 147), Size = new System.Drawing.Size(100, 23), Maximum = 999, Value = 10 };

            this.label5 = new System.Windows.Forms.Label { Text = "Max Araç:", Location = new System.Drawing.Point(250, 150), AutoSize = true };
            this.numMaxArac = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(330, 147), Size = new System.Drawing.Size(100, 23), Maximum = 9999, Value = 100 };

            this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] { 
                lblFirmaAdi, txtFirmaAdi, label1, txtYetkili, label2, txtEmail, 
                label3, txtTelefon, label4, numMaxKullanici, label5, numMaxArac 
            });

            // Oluþtur Butonu
            this.btnOlustur = new System.Windows.Forms.Button();
            this.btnOlustur.Text = "?? LÝSANS OLUÞTUR";
            this.btnOlustur.Location = new System.Drawing.Point(20, 310);
            this.btnOlustur.Size = new System.Drawing.Size(1020, 50);
            this.btnOlustur.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnOlustur.ForeColor = System.Drawing.Color.White;
            this.btnOlustur.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOlustur.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnOlustur.Click += new System.EventHandler(this.btnOlustur_Click);

            // Sonuç GroupBox
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox3.Text = "Oluþturulan Lisans";
            this.groupBox3.Location = new System.Drawing.Point(20, 370);
            this.groupBox3.Size = new System.Drawing.Size(1020, 320);

            this.txtLisansBilgi = new System.Windows.Forms.TextBox();
            this.txtLisansBilgi.Multiline = true;
            this.txtLisansBilgi.ReadOnly = true;
            this.txtLisansBilgi.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLisansBilgi.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLisansBilgi.Location = new System.Drawing.Point(20, 25);
            this.txtLisansBilgi.Size = new System.Drawing.Size(980, 180);

            this.txtLisansAnahtari = new System.Windows.Forms.TextBox();
            this.txtLisansAnahtari.Multiline = true;
            this.txtLisansAnahtari.ReadOnly = true;
            this.txtLisansAnahtari.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtLisansAnahtari.Location = new System.Drawing.Point(20, 210);
            this.txtLisansAnahtari.Size = new System.Drawing.Size(980, 60);

            this.btnKopyala = new System.Windows.Forms.Button { Text = "?? Panoya Kopyala", Location = new System.Drawing.Point(20, 280), Size = new System.Drawing.Size(480, 30), Enabled = false };
            this.btnKopyala.Click += new System.EventHandler(this.btnKopyala_Click);

            this.btnKaydet = new System.Windows.Forms.Button { Text = "?? Dosyaya Kaydet", Location = new System.Drawing.Point(520, 280), Size = new System.Drawing.Size(480, 30), Enabled = false };
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);

            this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] { txtLisansBilgi, txtLisansAnahtari, btnKopyala, btnKaydet });

            this.tabYeniLisans.Controls.AddRange(new System.Windows.Forms.Control[] { groupBox1, groupBox2, btnOlustur, groupBox3 });
        }

        private void InitializeDogrulaTab()
        {
            // Doðrula GroupBox
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox4.Text = "Lisans Anahtarý";
            this.groupBox4.Location = new System.Drawing.Point(20, 20);
            this.groupBox4.Size = new System.Drawing.Size(1020, 130);

            this.label6 = new System.Windows.Forms.Label { Text = "Doðrulanacak Lisans Anahtarýný Girin:", Location = new System.Drawing.Point(20, 30), AutoSize = true };
            this.txtDogrulaAnahtar = new System.Windows.Forms.TextBox();
            this.txtDogrulaAnahtar.Multiline = true;
            this.txtDogrulaAnahtar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDogrulaAnahtar.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDogrulaAnahtar.Location = new System.Drawing.Point(20, 50);
            this.txtDogrulaAnahtar.Size = new System.Drawing.Size(980, 60);

            this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] { label6, txtDogrulaAnahtar });

            // Doðrula Butonu
            this.btnDogrula = new System.Windows.Forms.Button();
            this.btnDogrula.Text = "? LÝSANSI DOÐRULA";
            this.btnDogrula.Location = new System.Drawing.Point(20, 160);
            this.btnDogrula.Size = new System.Drawing.Size(1020, 50);
            this.btnDogrula.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnDogrula.ForeColor = System.Drawing.Color.White;
            this.btnDogrula.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDogrula.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnDogrula.Click += new System.EventHandler(this.btnDogrula_Click);

            // Sonuç GroupBox
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox5.Text = "Doðrulama Sonucu";
            this.groupBox5.Location = new System.Drawing.Point(20, 220);
            this.groupBox5.Size = new System.Drawing.Size(1020, 450);

            this.txtDogrulaSonuc = new System.Windows.Forms.TextBox();
            this.txtDogrulaSonuc.Multiline = true;
            this.txtDogrulaSonuc.ReadOnly = true;
            this.txtDogrulaSonuc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDogrulaSonuc.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtDogrulaSonuc.Location = new System.Drawing.Point(20, 25);
            this.txtDogrulaSonuc.Size = new System.Drawing.Size(980, 400);

            this.groupBox5.Controls.Add(txtDogrulaSonuc);

            this.tabDogrula.Controls.AddRange(new System.Windows.Forms.Control[] { groupBox4, btnDogrula, groupBox5 });
        }

        private void InitializeLisansListesiTab()
        {
            // Özet Panel
            this.panelOzet = new System.Windows.Forms.Panel();
            this.panelOzet.Location = new System.Drawing.Point(10, 10);
            this.panelOzet.Size = new System.Drawing.Size(1040, 60);
            this.panelOzet.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.panelOzet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.lblToplamLisans = new System.Windows.Forms.Label { Text = "Toplam: 0", Location = new System.Drawing.Point(20, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold) };
            this.lblAktifLisans = new System.Windows.Forms.Label { Text = "Aktif: 0", Location = new System.Drawing.Point(200, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.Green };
            this.lblYaklasanLisans = new System.Windows.Forms.Label { Text = "Süresi Yaklaþan (30 gün): 0", Location = new System.Drawing.Point(350, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.Orange };
            this.lblDolmusLisans = new System.Windows.Forms.Label { Text = "Süresi Dolmuþ: 0", Location = new System.Drawing.Point(650, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.Red };

            this.panelOzet.Controls.AddRange(new System.Windows.Forms.Control[] { lblToplamLisans, lblAktifLisans, lblYaklasanLisans, lblDolmusLisans });

            // DataGridView
            this.dataGridLisanslar = new System.Windows.Forms.DataGridView();
            this.dataGridLisanslar.Location = new System.Drawing.Point(10, 80);
            this.dataGridLisanslar.Size = new System.Drawing.Size(1040, 520);
            this.dataGridLisanslar.AllowUserToAddRows = false;
            this.dataGridLisanslar.AllowUserToDeleteRows = false;
            this.dataGridLisanslar.ReadOnly = true;
            this.dataGridLisanslar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridLisanslar.MultiSelect = false;
            this.dataGridLisanslar.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridLisanslar.BackgroundColor = System.Drawing.Color.White;

            // Butonlar Panel
            this.panelButonlar = new System.Windows.Forms.Panel();
            this.panelButonlar.Location = new System.Drawing.Point(10, 610);
            this.panelButonlar.Size = new System.Drawing.Size(1040, 50);

            this.btnYenile = new System.Windows.Forms.Button { Text = "?? Lisansý Yenile", Location = new System.Drawing.Point(0, 5), Size = new System.Drawing.Size(130, 40), BackColor = System.Drawing.Color.FromArgb(40, 167, 69), ForeColor = System.Drawing.Color.White, FlatStyle = System.Windows.Forms.FlatStyle.Flat };
            this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);

            this.btnLisansDetay = new System.Windows.Forms.Button { Text = "?? Detay", Location = new System.Drawing.Point(140, 5), Size = new System.Drawing.Size(100, 40), FlatStyle = System.Windows.Forms.FlatStyle.Flat };
            this.btnLisansDetay.Click += new System.EventHandler(this.btnLisansDetay_Click);

            this.btnLisansAnahtariKopyala = new System.Windows.Forms.Button { Text = "?? Anahtar Kopyala", Location = new System.Drawing.Point(250, 5), Size = new System.Drawing.Size(140, 40), FlatStyle = System.Windows.Forms.FlatStyle.Flat };
            this.btnLisansAnahtariKopyala.Click += new System.EventHandler(this.btnLisansAnahtariKopyala_Click);

            this.btnLisansSil = new System.Windows.Forms.Button { Text = "?? Sil", Location = new System.Drawing.Point(400, 5), Size = new System.Drawing.Size(80, 40), BackColor = System.Drawing.Color.FromArgb(220, 53, 69), ForeColor = System.Drawing.Color.White, FlatStyle = System.Windows.Forms.FlatStyle.Flat };
            this.btnLisansSil.Click += new System.EventHandler(this.btnLisansSil_Click);

            this.btnListeYenile = new System.Windows.Forms.Button { Text = "?? Yenile", Location = new System.Drawing.Point(780, 5), Size = new System.Drawing.Size(100, 40), FlatStyle = System.Windows.Forms.FlatStyle.Flat };
            this.btnListeYenile.Click += new System.EventHandler(this.btnListeYenile_Click);

            this.btnExport = new System.Windows.Forms.Button { Text = "?? Dýþa Aktar", Location = new System.Drawing.Point(890, 5), Size = new System.Drawing.Size(140, 40), BackColor = System.Drawing.Color.FromArgb(0, 123, 255), ForeColor = System.Drawing.Color.White, FlatStyle = System.Windows.Forms.FlatStyle.Flat };
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);

            this.panelButonlar.Controls.AddRange(new System.Windows.Forms.Control[] { btnYenile, btnLisansDetay, btnLisansAnahtariKopyala, btnLisansSil, btnListeYenile, btnExport });

            this.tabLisansListesi.Controls.AddRange(new System.Windows.Forms.Control[] { panelOzet, dataGridLisanslar, panelButonlar });
        }

        // Kontroller
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabYeniLisans;
        private System.Windows.Forms.TabPage tabDogrula;
        private System.Windows.Forms.TabPage tabLisansListesi;

        // Yeni Lisans Sekmesi
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioTrial;
        private System.Windows.Forms.RadioButton radioStandard;
        private System.Windows.Forms.RadioButton radioProfessional;
        private System.Windows.Forms.RadioButton radioEnterprise;
        private System.Windows.Forms.NumericUpDown numYil;
        private System.Windows.Forms.NumericUpDown numAy;
        private System.Windows.Forms.NumericUpDown numGun;
        private System.Windows.Forms.Label lblYil;
        private System.Windows.Forms.Label lblAy;
        private System.Windows.Forms.Label lblGun;
        private System.Windows.Forms.Label lblMusteriMakineKodu;
        private System.Windows.Forms.TextBox txtMusteriMakineKodu;
        private System.Windows.Forms.Button btnMakineKoduAl;
        private System.Windows.Forms.Label lblFirmaAdi;
        private System.Windows.Forms.TextBox txtFirmaAdi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtYetkili;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTelefon;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numMaxKullanici;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numMaxArac;
        private System.Windows.Forms.Button btnOlustur;
        private System.Windows.Forms.TextBox txtLisansBilgi;
        private System.Windows.Forms.TextBox txtLisansAnahtari;
        private System.Windows.Forms.Button btnKopyala;
        private System.Windows.Forms.Button btnKaydet;

        // Doðrula Sekmesi
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDogrulaAnahtar;
        private System.Windows.Forms.Button btnDogrula;
        private System.Windows.Forms.TextBox txtDogrulaSonuc;

        // Lisans Listesi Sekmesi
        private System.Windows.Forms.Panel panelOzet;
        private System.Windows.Forms.Label lblToplamLisans;
        private System.Windows.Forms.Label lblAktifLisans;
        private System.Windows.Forms.Label lblYaklasanLisans;
        private System.Windows.Forms.Label lblDolmusLisans;
        private System.Windows.Forms.DataGridView dataGridLisanslar;
        private System.Windows.Forms.Panel panelButonlar;
        private System.Windows.Forms.Button btnYenile;
        private System.Windows.Forms.Button btnLisansDetay;
        private System.Windows.Forms.Button btnLisansAnahtariKopyala;
        private System.Windows.Forms.Button btnLisansSil;
        private System.Windows.Forms.Button btnListeYenile;
        private System.Windows.Forms.Button btnExport;
    }
}

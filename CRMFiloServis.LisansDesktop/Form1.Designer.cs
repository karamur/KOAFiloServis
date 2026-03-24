namespace CRMFiloServis.LisansDesktop
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Yeni kontroller
        private System.Windows.Forms.NumericUpDown numGun;
        private System.Windows.Forms.NumericUpDown numAy;
        private System.Windows.Forms.NumericUpDown numYil;
        private System.Windows.Forms.Label lblGun;
        private System.Windows.Forms.Label lblAy;
        private System.Windows.Forms.Label lblYil;
        
        // Makine kodu kontrolleri
        private System.Windows.Forms.TextBox txtMusteriMakineKodu;
        private System.Windows.Forms.Label lblMusteriMakineKodu;
        private System.Windows.Forms.Button btnMakineKoduAl;

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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabYeniLisans = new System.Windows.Forms.TabPage();
            this.tabDogrula = new System.Windows.Forms.TabPage();

            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioEnterprise = new System.Windows.Forms.RadioButton();
            this.radioProfessional = new System.Windows.Forms.RadioButton();
            this.radioStandard = new System.Windows.Forms.RadioButton();
            this.radioTrial = new System.Windows.Forms.RadioButton();

            // Süre kontrolleri
            this.numGun = new System.Windows.Forms.NumericUpDown();
            this.numAy = new System.Windows.Forms.NumericUpDown();
            this.numYil = new System.Windows.Forms.NumericUpDown();
            this.lblGun = new System.Windows.Forms.Label();
            this.lblAy = new System.Windows.Forms.Label();
            this.lblYil = new System.Windows.Forms.Label();
            
            // Makine kodu kontrolleri
            this.txtMusteriMakineKodu = new System.Windows.Forms.TextBox();
            this.lblMusteriMakineKodu = new System.Windows.Forms.Label();
            this.btnMakineKoduAl = new System.Windows.Forms.Button();

            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numMaxArac = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numMaxKullanici = new System.Windows.Forms.NumericUpDown();
            this.txtTelefon = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtYetkili = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFirmaAdi = new System.Windows.Forms.TextBox();
            this.lblFirmaAdi = new System.Windows.Forms.Label();

            this.btnOlustur = new System.Windows.Forms.Button();

            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnKopyala = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.txtLisansAnahtari = new System.Windows.Forms.TextBox();
            this.txtLisansBilgi = new System.Windows.Forms.TextBox();

            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtDogrulaAnahtar = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnDogrula = new System.Windows.Forms.Button();

            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtDogrulaSonuc = new System.Windows.Forms.TextBox();

            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();

            this.tabControl1.SuspendLayout();
            this.tabYeniLisans.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxArac)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxKullanici)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGun)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYil)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabDogrula.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabYeniLisans);
            this.tabControl1.Controls.Add(this.tabDogrula);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1000, 700);
            this.tabControl1.TabIndex = 0;

            // 
            // tabYeniLisans
            // 
            this.tabYeniLisans.Controls.Add(this.groupBox3);
            this.tabYeniLisans.Controls.Add(this.btnOlustur);
            this.tabYeniLisans.Controls.Add(this.groupBox2);
            this.tabYeniLisans.Controls.Add(this.groupBox1);
            this.tabYeniLisans.Location = new System.Drawing.Point(4, 24);
            this.tabYeniLisans.Name = "tabYeniLisans";
            this.tabYeniLisans.Padding = new System.Windows.Forms.Padding(3);
            this.tabYeniLisans.Size = new System.Drawing.Size(992, 672);
            this.tabYeniLisans.TabIndex = 0;
            this.tabYeniLisans.Text = "Yeni Lisans Oluþtur";
            this.tabYeniLisans.UseVisualStyleBackColor = true;

            // 
            // groupBox1 - Lisans Tipi ve Süre
            // 
            this.groupBox1.Controls.Add(this.btnMakineKoduAl);
            this.groupBox1.Controls.Add(this.txtMusteriMakineKodu);
            this.groupBox1.Controls.Add(this.lblMusteriMakineKodu);
            this.groupBox1.Controls.Add(this.lblYil);
            this.groupBox1.Controls.Add(this.numYil);
            this.groupBox1.Controls.Add(this.lblAy);
            this.groupBox1.Controls.Add(this.numAy);
            this.groupBox1.Controls.Add(this.lblGun);
            this.groupBox1.Controls.Add(this.numGun);
            this.groupBox1.Controls.Add(this.radioEnterprise);
            this.groupBox1.Controls.Add(this.radioProfessional);
            this.groupBox1.Controls.Add(this.radioStandard);
            this.groupBox1.Controls.Add(this.radioTrial);
            this.groupBox1.Location = new System.Drawing.Point(20, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(450, 280);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lisans Tipi ve Süre";

            // 
            // radioTrial
            // 
            this.radioTrial.AutoSize = true;
            this.radioTrial.Location = new System.Drawing.Point(20, 30);
            this.radioTrial.Name = "radioTrial";
            this.radioTrial.Size = new System.Drawing.Size(55, 19);
            this.radioTrial.TabIndex = 0;
            this.radioTrial.Text = "Trial";
            this.radioTrial.UseVisualStyleBackColor = true;

            // 
            // radioStandard
            // 
            this.radioStandard.AutoSize = true;
            this.radioStandard.Checked = true;
            this.radioStandard.Location = new System.Drawing.Point(20, 55);
            this.radioStandard.Name = "radioStandard";
            this.radioStandard.Size = new System.Drawing.Size(75, 19);
            this.radioStandard.TabIndex = 1;
            this.radioStandard.TabStop = true;
            this.radioStandard.Text = "Standard";
            this.radioStandard.UseVisualStyleBackColor = true;

            // 
            // radioProfessional
            // 
            this.radioProfessional.AutoSize = true;
            this.radioProfessional.Location = new System.Drawing.Point(20, 80);
            this.radioProfessional.Name = "radioProfessional";
            this.radioProfessional.Size = new System.Drawing.Size(95, 19);
            this.radioProfessional.TabIndex = 2;
            this.radioProfessional.Text = "Professional";
            this.radioProfessional.UseVisualStyleBackColor = true;

            // 
            // radioEnterprise
            // 
            this.radioEnterprise.AutoSize = true;
            this.radioEnterprise.Location = new System.Drawing.Point(20, 105);
            this.radioEnterprise.Name = "radioEnterprise";
            this.radioEnterprise.Size = new System.Drawing.Size(85, 19);
            this.radioEnterprise.TabIndex = 3;
            this.radioEnterprise.Text = "Enterprise";
            this.radioEnterprise.UseVisualStyleBackColor = true;

            // 
            // lblYil
            // 
            this.lblYil.AutoSize = true;
            this.lblYil.Location = new System.Drawing.Point(20, 145);
            this.lblYil.Name = "lblYil";
            this.lblYil.Size = new System.Drawing.Size(23, 15);
            this.lblYil.TabIndex = 4;
            this.lblYil.Text = "Yýl:";

            // 
            // numYil
            // 
            this.numYil.Location = new System.Drawing.Point(70, 143);
            this.numYil.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numYil.Name = "numYil";
            this.numYil.Size = new System.Drawing.Size(80, 23);
            this.numYil.TabIndex = 5;
            this.numYil.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // lblAy
            // 
            this.lblAy.AutoSize = true;
            this.lblAy.Location = new System.Drawing.Point(170, 145);
            this.lblAy.Name = "lblAy";
            this.lblAy.Size = new System.Drawing.Size(24, 15);
            this.lblAy.TabIndex = 6;
            this.lblAy.Text = "Ay:";

            // 
            // numAy
            // 
            this.numAy.Location = new System.Drawing.Point(210, 143);
            this.numAy.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            this.numAy.Name = "numAy";
            this.numAy.Size = new System.Drawing.Size(80, 23);
            this.numAy.TabIndex = 7;

            // 
            // lblGun
            // 
            this.lblGun.AutoSize = true;
            this.lblGun.Location = new System.Drawing.Point(310, 145);
            this.lblGun.Name = "lblGun";
            this.lblGun.Size = new System.Drawing.Size(32, 15);
            this.lblGun.TabIndex = 8;
            this.lblGun.Text = "Gün:";

            // 
            // numGun
            // 
            this.numGun.Location = new System.Drawing.Point(350, 143);
            this.numGun.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
            this.numGun.Name = "numGun";
            this.numGun.Size = new System.Drawing.Size(80, 23);
            this.numGun.TabIndex = 9;

            // 
            // lblMusteriMakineKodu
            // 
            this.lblMusteriMakineKodu.AutoSize = true;
            this.lblMusteriMakineKodu.Location = new System.Drawing.Point(20, 175);
            this.lblMusteriMakineKodu.Name = "lblMusteriMakineKodu";
            this.lblMusteriMakineKodu.Size = new System.Drawing.Size(130, 15);
            this.lblMusteriMakineKodu.TabIndex = 10;
            this.lblMusteriMakineKodu.Text = "Müþteri Makine Kodu:";

            // 
            // txtMusteriMakineKodu
            // 
            this.txtMusteriMakineKodu.Location = new System.Drawing.Point(160, 172);
            this.txtMusteriMakineKodu.Name = "txtMusteriMakineKodu";
            this.txtMusteriMakineKodu.Size = new System.Drawing.Size(220, 23);
            this.txtMusteriMakineKodu.TabIndex = 11;

            // 
            // btnMakineKoduAl
            // 
            this.btnMakineKoduAl.Location = new System.Drawing.Point(390, 172);
            this.btnMakineKoduAl.Name = "btnMakineKoduAl";
            this.btnMakineKoduAl.Size = new System.Drawing.Size(50, 30);
            this.btnMakineKoduAl.TabIndex = 12;
            this.btnMakineKoduAl.Text = "Al";
            this.btnMakineKoduAl.UseVisualStyleBackColor = true;
            this.btnMakineKoduAl.Click += new System.EventHandler(this.btnMakineKoduAl_Click);

            // 
            // groupBox2 - Firma Bilgileri
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.numMaxArac);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.numMaxKullanici);
            this.groupBox2.Controls.Add(this.txtTelefon);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtEmail);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtYetkili);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtFirmaAdi);
            this.groupBox2.Controls.Add(this.lblFirmaAdi);
            this.groupBox2.Location = new System.Drawing.Point(490, 20);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(480, 280);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Firma Bilgileri";

            // 
            // lblFirmaAdi
            // 
            this.lblFirmaAdi.AutoSize = true;
            this.lblFirmaAdi.Location = new System.Drawing.Point(20, 30);
            this.lblFirmaAdi.Name = "lblFirmaAdi";
            this.lblFirmaAdi.Size = new System.Drawing.Size(65, 15);
            this.lblFirmaAdi.TabIndex = 0;
            this.lblFirmaAdi.Text = "Firma Adý:";

            // 
            // txtFirmaAdi
            // 
            this.txtFirmaAdi.Location = new System.Drawing.Point(130, 27);
            this.txtFirmaAdi.Name = "txtFirmaAdi";
            this.txtFirmaAdi.Size = new System.Drawing.Size(330, 23);
            this.txtFirmaAdi.TabIndex = 1;

            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Yetkili Kiþi:";

            // 
            // txtYetkili
            // 
            this.txtYetkili.Location = new System.Drawing.Point(130, 57);
            this.txtYetkili.Name = "txtYetkili";
            this.txtYetkili.Size = new System.Drawing.Size(330, 23);
            this.txtYetkili.TabIndex = 3;

            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Email:";

            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(130, 87);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(330, 23);
            this.txtEmail.TabIndex = 5;

            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Telefon:";

            // 
            // txtTelefon
            // 
            this.txtTelefon.Location = new System.Drawing.Point(130, 117);
            this.txtTelefon.Name = "txtTelefon";
            this.txtTelefon.Size = new System.Drawing.Size(330, 23);
            this.txtTelefon.TabIndex = 7;

            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Max Kullanýcý:";

            // 
            // numMaxKullanici
            // 
            this.numMaxKullanici.Location = new System.Drawing.Point(130, 147);
            this.numMaxKullanici.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            this.numMaxKullanici.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxKullanici.Name = "numMaxKullanici";
            this.numMaxKullanici.Size = new System.Drawing.Size(100, 23);
            this.numMaxKullanici.TabIndex = 9;
            this.numMaxKullanici.Value = new decimal(new int[] { 10, 0, 0, 0 });

            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(250, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Max Araç:";

            // 
            // numMaxArac
            // 
            this.numMaxArac.Location = new System.Drawing.Point(330, 147);
            this.numMaxArac.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            this.numMaxArac.Name = "numMaxArac";
            this.numMaxArac.Size = new System.Drawing.Size(100, 23);
            this.numMaxArac.TabIndex = 11;
            this.numMaxArac.Value = new decimal(new int[] { 50, 0, 0, 0 });

            // 
            // btnOlustur
            // 
            this.btnOlustur.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnOlustur.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOlustur.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnOlustur.ForeColor = System.Drawing.Color.White;
            this.btnOlustur.Location = new System.Drawing.Point(20, 320);
            this.btnOlustur.Name = "btnOlustur";
            this.btnOlustur.Size = new System.Drawing.Size(950, 50);
            this.btnOlustur.TabIndex = 2;
            this.btnOlustur.Text = "?? LÝSANS OLUÞTUR";
            this.btnOlustur.UseVisualStyleBackColor = false;
            this.btnOlustur.Click += new System.EventHandler(this.btnOlustur_Click);

            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtLisansBilgi);
            this.groupBox3.Controls.Add(this.txtLisansAnahtari);
            this.groupBox3.Controls.Add(this.btnKaydet);
            this.groupBox3.Controls.Add(this.btnKopyala);
            this.groupBox3.Location = new System.Drawing.Point(20, 390);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(950, 260);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Lisans Bilgileri";

            // 
            // txtLisansBilgi
            // 
            this.txtLisansBilgi.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLisansBilgi.Location = new System.Drawing.Point(20, 30);
            this.txtLisansBilgi.Multiline = true;
            this.txtLisansBilgi.Name = "txtLisansBilgi";
            this.txtLisansBilgi.ReadOnly = true;
            this.txtLisansBilgi.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLisansBilgi.Size = new System.Drawing.Size(910, 180);
            this.txtLisansBilgi.TabIndex = 0;

            // 
            // txtLisansAnahtari
            // 
            this.txtLisansAnahtari.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtLisansAnahtari.Location = new System.Drawing.Point(20, 220);
            this.txtLisansAnahtari.Multiline = true;
            this.txtLisansAnahtari.Name = "txtLisansAnahtari";
            this.txtLisansAnahtari.ReadOnly = true;
            this.txtLisansAnahtari.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLisansAnahtari.Size = new System.Drawing.Size(910, 60);
            this.txtLisansAnahtari.TabIndex = 1;

            // 
            // btnKopyala
            // 
            this.btnKopyala.Enabled = false;
            this.btnKopyala.Location = new System.Drawing.Point(20, 295);
            this.btnKopyala.Name = "btnKopyala";
            this.btnKopyala.Size = new System.Drawing.Size(450, 30);
            this.btnKopyala.TabIndex = 2;
            this.btnKopyala.Text = "?? Panoya Kopyala";
            this.btnKopyala.UseVisualStyleBackColor = true;
            this.btnKopyala.Click += new System.EventHandler(this.btnKopyala_Click);

            // 
            // btnKaydet
            // 
            this.btnKaydet.Enabled = false;
            this.btnKaydet.Location = new System.Drawing.Point(480, 295);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(450, 30);
            this.btnKaydet.TabIndex = 3;
            this.btnKaydet.Text = "?? Dosyaya Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);

            // 
            // tabDogrula
            // 
            this.tabDogrula.Controls.Add(this.groupBox5);
            this.tabDogrula.Controls.Add(this.btnDogrula);
            this.tabDogrula.Controls.Add(this.groupBox4);
            this.tabDogrula.Location = new System.Drawing.Point(4, 24);
            this.tabDogrula.Name = "tabDogrula";
            this.tabDogrula.Padding = new System.Windows.Forms.Padding(3);
            this.tabDogrula.Size = new System.Drawing.Size(992, 672);
            this.tabDogrula.TabIndex = 1;
            this.tabDogrula.Text = "Lisans Doðrula";
            this.tabDogrula.UseVisualStyleBackColor = true;

            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.txtDogrulaAnahtar);
            this.groupBox4.Location = new System.Drawing.Point(20, 20);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(950, 150);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Lisans Anahtarý";

            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(200, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "Doðrulanacak Lisans Anahtarýný Girin:";

            // 
            // txtDogrulaAnahtar
            // 
            this.txtDogrulaAnahtar.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDogrulaAnahtar.Location = new System.Drawing.Point(20, 55);
            this.txtDogrulaAnahtar.Multiline = true;
            this.txtDogrulaAnahtar.Name = "txtDogrulaAnahtar";
            this.txtDogrulaAnahtar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDogrulaAnahtar.Size = new System.Drawing.Size(910, 75);
            this.txtDogrulaAnahtar.TabIndex = 1;

            // 
            // btnDogrula
            // 
            this.btnDogrula.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnDogrula.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDogrula.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnDogrula.ForeColor = System.Drawing.Color.White;
            this.btnDogrula.Location = new System.Drawing.Point(20, 190);
            this.btnDogrula.Name = "btnDogrula";
            this.btnDogrula.Size = new System.Drawing.Size(950, 50);
            this.btnDogrula.TabIndex = 1;
            this.btnDogrula.Text = "? LÝSANSI DOÐRULA";
            this.btnDogrula.UseVisualStyleBackColor = false;
            this.btnDogrula.Click += new System.EventHandler(this.btnDogrula_Click);

            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtDogrulaSonuc);
            this.groupBox5.Location = new System.Drawing.Point(20, 260);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(950, 390);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Doðrulama Sonucu";

            // 
            // txtDogrulaSonuc
            // 
            this.txtDogrulaSonuc.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDogrulaSonuc.Location = new System.Drawing.Point(20, 30);
            this.txtDogrulaSonuc.Multiline = true;
            this.txtDogrulaSonuc.Name = "txtDogrulaSonuc";
            this.txtDogrulaSonuc.ReadOnly = true;
            this.txtDogrulaSonuc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDogrulaSonuc.Size = new System.Drawing.Size(910, 340);
            this.txtDogrulaSonuc.TabIndex = 0;

            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 678);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip1.TabIndex = 1;

            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 17);
            this.toolStripStatusLabel1.Text = "CRM Filo Servis - Lisans Oluþturucu v2.0";

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CRM Filo Servis - Lisans Oluþturucu";

            this.tabControl1.ResumeLayout(false);
            this.tabYeniLisans.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxArac)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxKullanici)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGun)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYil)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabDogrula.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Kontroller
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabYeniLisans;
        private System.Windows.Forms.TabPage tabDogrula;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioEnterprise;
        private System.Windows.Forms.RadioButton radioProfessional;
        private System.Windows.Forms.RadioButton radioStandard;
        private System.Windows.Forms.RadioButton radioTrial;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numMaxArac;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numMaxKullanici;
        private System.Windows.Forms.TextBox txtTelefon;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtYetkili;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFirmaAdi;
        private System.Windows.Forms.Label lblFirmaAdi;
        private System.Windows.Forms.Button btnOlustur;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnKopyala;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.TextBox txtLisansAnahtari;
        private System.Windows.Forms.TextBox txtLisansBilgi;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtDogrulaAnahtar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnDogrula;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtDogrulaSonuc;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

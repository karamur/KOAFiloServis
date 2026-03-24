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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabYeniLisans = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioEnterprise = new System.Windows.Forms.RadioButton();
            this.radioProfessional = new System.Windows.Forms.RadioButton();
            this.radioStandard = new System.Windows.Forms.RadioButton();
            this.radioTrial = new System.Windows.Forms.RadioButton();
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
            this.tabDogrula = new System.Windows.Forms.TabPage();
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
            this.tabControl1.Size = new System.Drawing.Size(900, 650);
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
            this.tabYeniLisans.Size = new System.Drawing.Size(892, 622);
            this.tabYeniLisans.TabIndex = 0;
            this.tabYeniLisans.Text = "Yeni Lisans Oluþtur";
            this.tabYeniLisans.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioEnterprise);
            this.groupBox1.Controls.Add(this.radioProfessional);
            this.groupBox1.Controls.Add(this.radioStandard);
            this.groupBox1.Controls.Add(this.radioTrial);
            this.groupBox1.Location = new System.Drawing.Point(20, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(400, 130);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lisans Tipi";
            // 
            // radioEnterprise
            // 
            this.radioEnterprise.AutoSize = true;
            this.radioEnterprise.Location = new System.Drawing.Point(20, 100);
            this.radioEnterprise.Name = "radioEnterprise";
            this.radioEnterprise.Size = new System.Drawing.Size(150, 19);
            this.radioEnterprise.TabIndex = 3;
            this.radioEnterprise.Text = "Enterprise (5 yýl)";
            this.radioEnterprise.UseVisualStyleBackColor = true;
            // 
            // radioProfessional
            // 
            this.radioProfessional.AutoSize = true;
            this.radioProfessional.Location = new System.Drawing.Point(20, 75);
            this.radioProfessional.Name = "radioProfessional";
            this.radioProfessional.Size = new System.Drawing.Size(155, 19);
            this.radioProfessional.TabIndex = 2;
            this.radioProfessional.Text = "Professional (2 yýl)";
            this.radioProfessional.UseVisualStyleBackColor = true;
            // 
            // radioStandard
            // 
            this.radioStandard.AutoSize = true;
            this.radioStandard.Checked = true;
            this.radioStandard.Location = new System.Drawing.Point(20, 50);
            this.radioStandard.Name = "radioStandard";
            this.radioStandard.Size = new System.Drawing.Size(135, 19);
            this.radioStandard.TabIndex = 1;
            this.radioStandard.TabStop = true;
            this.radioStandard.Text = "Standard (1 yýl)";
            this.radioStandard.UseVisualStyleBackColor = true;
            // 
            // radioTrial
            // 
            this.radioTrial.AutoSize = true;
            this.radioTrial.Location = new System.Drawing.Point(20, 25);
            this.radioTrial.Name = "radioTrial";
            this.radioTrial.Size = new System.Drawing.Size(120, 19);
            this.radioTrial.TabIndex = 0;
            this.radioTrial.Text = "Trial (30 gün)";
            this.radioTrial.UseVisualStyleBackColor = true;
            // 
            // groupBox2
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
            this.groupBox2.Location = new System.Drawing.Point(440, 20);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(430, 240);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Firma Bilgileri";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 210);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Max Araç Sayýsý:";
            // 
            // numMaxArac
            // 
            this.numMaxArac.Location = new System.Drawing.Point(150, 208);
            this.numMaxArac.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numMaxArac.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxArac.Name = "numMaxArac";
            this.numMaxArac.Size = new System.Drawing.Size(260, 23);
            this.numMaxArac.TabIndex = 10;
            this.numMaxArac.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 178);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Max Kullanýcý Sayýsý:";
            // 
            // numMaxKullanici
            // 
            this.numMaxKullanici.Location = new System.Drawing.Point(150, 176);
            this.numMaxKullanici.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMaxKullanici.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxKullanici.Name = "numMaxKullanici";
            this.numMaxKullanici.Size = new System.Drawing.Size(260, 23);
            this.numMaxKullanici.TabIndex = 8;
            this.numMaxKullanici.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // txtTelefon
            // 
            this.txtTelefon.Location = new System.Drawing.Point(150, 140);
            this.txtTelefon.Name = "txtTelefon";
            this.txtTelefon.PlaceholderText = "+90 212 000 00 00";
            this.txtTelefon.Size = new System.Drawing.Size(260, 23);
            this.txtTelefon.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Telefon:";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(150, 105);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderText = "info@firma.com";
            this.txtEmail.Size = new System.Drawing.Size(260, 23);
            this.txtEmail.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Email:";
            // 
            // txtYetkili
            // 
            this.txtYetkili.Location = new System.Drawing.Point(150, 70);
            this.txtYetkili.Name = "txtYetkili";
            this.txtYetkili.Size = new System.Drawing.Size(260, 23);
            this.txtYetkili.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Yetkili Kiþi:";
            // 
            // txtFirmaAdi
            // 
            this.txtFirmaAdi.Location = new System.Drawing.Point(150, 35);
            this.txtFirmaAdi.Name = "txtFirmaAdi";
            this.txtFirmaAdi.Size = new System.Drawing.Size(260, 23);
            this.txtFirmaAdi.TabIndex = 1;
            // 
            // lblFirmaAdi
            // 
            this.lblFirmaAdi.AutoSize = true;
            this.lblFirmaAdi.Location = new System.Drawing.Point(20, 38);
            this.lblFirmaAdi.Name = "lblFirmaAdi";
            this.lblFirmaAdi.Size = new System.Drawing.Size(63, 15);
            this.lblFirmaAdi.TabIndex = 0;
            this.lblFirmaAdi.Text = "Firma Adý:";
            // 
            // btnOlustur
            // 
            this.btnOlustur.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnOlustur.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOlustur.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnOlustur.ForeColor = System.Drawing.Color.White;
            this.btnOlustur.Location = new System.Drawing.Point(20, 170);
            this.btnOlustur.Name = "btnOlustur";
            this.btnOlustur.Size = new System.Drawing.Size(400, 40);
            this.btnOlustur.TabIndex = 2;
            this.btnOlustur.Text = "Lisans Oluþtur";
            this.btnOlustur.UseVisualStyleBackColor = false;
            this.btnOlustur.Click += new System.EventHandler(this.btnOlustur_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtLisansBilgi);
            this.groupBox3.Controls.Add(this.txtLisansAnahtari);
            this.groupBox3.Controls.Add(this.btnKaydet);
            this.groupBox3.Controls.Add(this.btnKopyala);
            this.groupBox3.Location = new System.Drawing.Point(20, 270);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(850, 330);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Lisans Bilgileri";
            // 
            // btnKopyala
            // 
            this.btnKopyala.Enabled = false;
            this.btnKopyala.Location = new System.Drawing.Point(650, 280);
            this.btnKopyala.Name = "btnKopyala";
            this.btnKopyala.Size = new System.Drawing.Size(180, 35);
            this.btnKopyala.TabIndex = 0;
            this.btnKopyala.Text = "Anahtarý Kopyala";
            this.btnKopyala.UseVisualStyleBackColor = true;
            this.btnKopyala.Click += new System.EventHandler(this.btnKopyala_Click);
            // 
            // btnKaydet
            // 
            this.btnKaydet.Enabled = false;
            this.btnKaydet.Location = new System.Drawing.Point(450, 280);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(180, 35);
            this.btnKaydet.TabIndex = 1;
            this.btnKaydet.Text = "Dosyaya Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // txtLisansAnahtari
            // 
            this.txtLisansAnahtari.BackColor = System.Drawing.Color.LightYellow;
            this.txtLisansAnahtari.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLisansAnahtari.Location = new System.Drawing.Point(20, 230);
            this.txtLisansAnahtari.Multiline = true;
            this.txtLisansAnahtari.Name = "txtLisansAnahtari";
            this.txtLisansAnahtari.ReadOnly = true;
            this.txtLisansAnahtari.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLisansAnahtari.Size = new System.Drawing.Size(810, 40);
            this.txtLisansAnahtari.TabIndex = 2;
            // 
            // txtLisansBilgi
            // 
            this.txtLisansBilgi.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtLisansBilgi.Location = new System.Drawing.Point(20, 30);
            this.txtLisansBilgi.Multiline = true;
            this.txtLisansBilgi.Name = "txtLisansBilgi";
            this.txtLisansBilgi.ReadOnly = true;
            this.txtLisansBilgi.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLisansBilgi.Size = new System.Drawing.Size(810, 190);
            this.txtLisansBilgi.TabIndex = 3;
            // 
            // tabDogrula
            // 
            this.tabDogrula.Controls.Add(this.groupBox5);
            this.tabDogrula.Controls.Add(this.btnDogrula);
            this.tabDogrula.Controls.Add(this.groupBox4);
            this.tabDogrula.Location = new System.Drawing.Point(4, 24);
            this.tabDogrula.Name = "tabDogrula";
            this.tabDogrula.Padding = new System.Windows.Forms.Padding(3);
            this.tabDogrula.Size = new System.Drawing.Size(892, 622);
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
            this.groupBox4.Size = new System.Drawing.Size(850, 140);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Lisans Anahtarý";
            // 
            // txtDogrulaAnahtar
            // 
            this.txtDogrulaAnahtar.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDogrulaAnahtar.Location = new System.Drawing.Point(20, 60);
            this.txtDogrulaAnahtar.Multiline = true;
            this.txtDogrulaAnahtar.Name = "txtDogrulaAnahtar";
            this.txtDogrulaAnahtar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDogrulaAnahtar.Size = new System.Drawing.Size(810, 60);
            this.txtDogrulaAnahtar.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(200, 15);
            this.label6.TabIndex = 1;
            this.label6.Text = "Lisans anahtarýný buraya yapýþtýrýn:";
            // 
            // btnDogrula
            // 
            this.btnDogrula.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnDogrula.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDogrula.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnDogrula.ForeColor = System.Drawing.Color.White;
            this.btnDogrula.Location = new System.Drawing.Point(20, 175);
            this.btnDogrula.Name = "btnDogrula";
            this.btnDogrula.Size = new System.Drawing.Size(850, 40);
            this.btnDogrula.TabIndex = 1;
            this.btnDogrula.Text = "Doðrula";
            this.btnDogrula.UseVisualStyleBackColor = false;
            this.btnDogrula.Click += new System.EventHandler(this.btnDogrula_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtDogrulaSonuc);
            this.groupBox5.Location = new System.Drawing.Point(20, 230);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(850, 370);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Doðrulama Sonucu";
            // 
            // txtDogrulaSonuc
            // 
            this.txtDogrulaSonuc.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtDogrulaSonuc.Location = new System.Drawing.Point(20, 30);
            this.txtDogrulaSonuc.Multiline = true;
            this.txtDogrulaSonuc.Name = "txtDogrulaSonuc";
            this.txtDogrulaSonuc.ReadOnly = true;
            this.txtDogrulaSonuc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDogrulaSonuc.Size = new System.Drawing.Size(810, 320);
            this.txtDogrulaSonuc.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 628);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(900, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(250, 17);
            this.toolStripStatusLabel1.Text = "CRM Filo Servis - Lisans Oluþturucu v1.0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
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

        private TabControl tabControl1;
        private TabPage tabYeniLisans;
        private TabPage tabDogrula;
        private GroupBox groupBox1;
        private RadioButton radioTrial;
        private RadioButton radioStandard;
        private RadioButton radioProfessional;
        private RadioButton radioEnterprise;
        private GroupBox groupBox2;
        private Label lblFirmaAdi;
        private TextBox txtFirmaAdi;
        private Label label1;
        private TextBox txtYetkili;
        private Label label2;
        private TextBox txtEmail;
        private Label label3;
        private TextBox txtTelefon;
        private NumericUpDown numMaxKullanici;
        private Label label4;
        private Label label5;
        private NumericUpDown numMaxArac;
        private Button btnOlustur;
        private GroupBox groupBox3;
        private TextBox txtLisansBilgi;
        private TextBox txtLisansAnahtari;
        private Button btnKaydet;
        private Button btnKopyala;
        private GroupBox groupBox4;
        private Label label6;
        private TextBox txtDogrulaAnahtar;
        private Button btnDogrula;
        private GroupBox groupBox5;
        private TextBox txtDogrulaSonuc;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}

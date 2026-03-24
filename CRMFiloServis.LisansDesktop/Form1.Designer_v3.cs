namespace CRMFiloServis.LisansDesktop;

/// <summary>
/// Improved Form1 Designer with modern C# 13/.NET 10 features.
/// To use this version:
/// 1. Rename this class to Form1 (remove _v3 suffix)
/// 2. Remove or rename Form1.Designer.cs
/// 3. Update Form1.cs to use this designer
/// </summary>
partial class Form1_v3 : Form
{
    private System.ComponentModel.IContainer? components = null;

    #region Control Declarations

    // Tab Controls
    private TabControl tabControl1 = null!;
    private TabPage tabYeniLisans = null!;
    private TabPage tabDogrula = null!;

    // License Type Group
    private GroupBox groupBox1 = null!;
    private RadioButton radioTrial = null!;
    private RadioButton radioStandard = null!;
    private RadioButton radioProfessional = null!;
    private RadioButton radioEnterprise = null!;

    // Duration Controls
    private NumericUpDown numYil = null!;
    private NumericUpDown numAy = null!;
    private NumericUpDown numGun = null!;
    private Label lblYil = null!;
    private Label lblAy = null!;
    private Label lblGun = null!;

    // Machine Code Controls
    private TextBox txtMusteriMakineKodu = null!;
    private Label lblMusteriMakineKodu = null!;
    private Button btnMakineKoduAl = null!;

    // Company Info Group
    private GroupBox groupBox2 = null!;
    private TextBox txtFirmaAdi = null!;
    private TextBox txtYetkili = null!;
    private TextBox txtEmail = null!;
    private TextBox txtTelefon = null!;
    private NumericUpDown numMaxKullanici = null!;
    private NumericUpDown numMaxArac = null!;
    private Label lblFirmaAdi = null!;
    private Label label1 = null!;
    private Label label2 = null!;
    private Label label3 = null!;
    private Label label4 = null!;
    private Label label5 = null!;

    // Generate Button
    private Button btnOlustur = null!;

    // License Output Group
    private GroupBox groupBox3 = null!;
    private TextBox txtLisansBilgi = null!;
    private TextBox txtLisansAnahtari = null!;
    private Button btnKopyala = null!;
    private Button btnKaydet = null!;

    // Validation Tab Controls
    private GroupBox groupBox4 = null!;
    private TextBox txtDogrulaAnahtar = null!;
    private Label label6 = null!;
    private Button btnDogrula = null!;
    private GroupBox groupBox5 = null!;
    private TextBox txtDogrulaSonuc = null!;

    // Status Bar
    private StatusStrip statusStrip1 = null!;
    private ToolStripStatusLabel toolStripStatusLabel1 = null!;

    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        InitializeTabControls();
        InitializeLicenseTypeGroup();
        InitializeCompanyInfoGroup();
        InitializeLicenseOutputGroup();
        InitializeValidationTab();
        InitializeStatusBar();
        ConfigureFormLayout();
    }

    private void InitializeTabControls()
    {
        tabControl1 = new TabControl
        {
            Dock = DockStyle.Fill,
            Name = "tabControl1"
        };

        tabYeniLisans = new TabPage
        {
            Name = "tabYeniLisans",
            Padding = new Padding(10),
            Text = "Yeni Lisans Oluþtur",
            UseVisualStyleBackColor = true
        };

        tabDogrula = new TabPage
        {
            Name = "tabDogrula",
            Padding = new Padding(10),
            Text = "Lisans Doðrula",
            UseVisualStyleBackColor = true
        };

        tabControl1.TabPages.AddRange([tabYeniLisans, tabDogrula]);
    }

    private void InitializeLicenseTypeGroup()
    {
        groupBox1 = new GroupBox
        {
            Name = "groupBox1",
            Text = "Lisans Tipi ve Süre",
            Location = new Point(20, 20),
            Size = new Size(450, 280),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        // Radio buttons for license type
        radioTrial = CreateRadioButton("radioTrial", "Trial", new Point(20, 30), 0);
        radioStandard = CreateRadioButton("radioStandard", "Standard", new Point(20, 55), 1, isChecked: true);
        radioProfessional = CreateRadioButton("radioProfessional", "Professional", new Point(20, 80), 2);
        radioEnterprise = CreateRadioButton("radioEnterprise", "Enterprise", new Point(20, 105), 3);

        // Duration controls
        lblYil = CreateLabel("lblYil", "Yýl:", new Point(20, 145));
        numYil = CreateNumericUpDown("numYil", new Point(70, 143), 99, 1);

        lblAy = CreateLabel("lblAy", "Ay:", new Point(170, 145));
        numAy = CreateNumericUpDown("numAy", new Point(210, 143), 12, 0);

        lblGun = CreateLabel("lblGun", "Gün:", new Point(310, 145));
        numGun = CreateNumericUpDown("numGun", new Point(350, 143), 365, 0);

        // Machine code controls
        lblMusteriMakineKodu = CreateLabel("lblMusteriMakineKodu", "Müþteri Makine Kodu:", new Point(20, 185));

        txtMusteriMakineKodu = new TextBox
        {
            Name = "txtMusteriMakineKodu",
            Location = new Point(160, 182),
            Size = new Size(220, 23)
        };

        btnMakineKoduAl = new Button
        {
            Name = "btnMakineKoduAl",
            Text = "Al",
            Location = new Point(390, 180),
            Size = new Size(50, 27)
        };
        btnMakineKoduAl.Click += BtnMakineKoduAl_Click;

        groupBox1.Controls.AddRange([
            radioTrial, radioStandard, radioProfessional, radioEnterprise,
            lblYil, numYil, lblAy, numAy, lblGun, numGun,
            lblMusteriMakineKodu, txtMusteriMakineKodu, btnMakineKoduAl
        ]);

        tabYeniLisans.Controls.Add(groupBox1);
    }

    private void InitializeCompanyInfoGroup()
    {
        groupBox2 = new GroupBox
        {
            Name = "groupBox2",
            Text = "Firma Bilgileri",
            Location = new Point(490, 20),
            Size = new Size(480, 280),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // Labels and TextBoxes
        lblFirmaAdi = CreateLabel("lblFirmaAdi", "Firma Adý:", new Point(20, 30));
        txtFirmaAdi = CreateTextBox("txtFirmaAdi", new Point(130, 27), 330);

        label1 = CreateLabel("label1", "Yetkili Kiþi:", new Point(20, 60));
        txtYetkili = CreateTextBox("txtYetkili", new Point(130, 57), 330);

        label2 = CreateLabel("label2", "Email:", new Point(20, 90));
        txtEmail = CreateTextBox("txtEmail", new Point(130, 87), 330);

        label3 = CreateLabel("label3", "Telefon:", new Point(20, 120));
        txtTelefon = CreateTextBox("txtTelefon", new Point(130, 117), 330);

        label4 = CreateLabel("label4", "Max Kullanýcý:", new Point(20, 150));
        numMaxKullanici = CreateNumericUpDown("numMaxKullanici", new Point(130, 147), 999, 10, minValue: 1, width: 100);

        label5 = CreateLabel("label5", "Max Araç:", new Point(250, 150));
        numMaxArac = CreateNumericUpDown("numMaxArac", new Point(330, 147), 999, 50, width: 100);

        groupBox2.Controls.AddRange([
            lblFirmaAdi, txtFirmaAdi,
            label1, txtYetkili,
            label2, txtEmail,
            label3, txtTelefon,
            label4, numMaxKullanici,
            label5, numMaxArac
        ]);

        // Generate Button
        btnOlustur = new Button
        {
            Name = "btnOlustur",
            Text = "?? LÝSANS OLUÞTUR",
            Location = new Point(20, 320),
            Size = new Size(950, 50),
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        btnOlustur.Click += BtnOlustur_Click;

        tabYeniLisans.Controls.AddRange([groupBox2, btnOlustur]);
    }

    private void InitializeLicenseOutputGroup()
    {
        groupBox3 = new GroupBox
        {
            Name = "groupBox3",
            Text = "Lisans Bilgileri",
            Location = new Point(20, 390),
            Size = new Size(950, 260),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        txtLisansBilgi = new TextBox
        {
            Name = "txtLisansBilgi",
            Location = new Point(20, 30),
            Size = new Size(910, 130),
            Font = new Font("Consolas", 9F),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        txtLisansAnahtari = new TextBox
        {
            Name = "txtLisansAnahtari",
            Location = new Point(20, 170),
            Size = new Size(910, 40),
            Font = new Font("Consolas", 8F),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        btnKopyala = new Button
        {
            Name = "btnKopyala",
            Text = "?? Panoya Kopyala",
            Location = new Point(20, 220),
            Size = new Size(445, 30),
            Enabled = false,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        btnKopyala.Click += BtnKopyala_Click;

        btnKaydet = new Button
        {
            Name = "btnKaydet",
            Text = "?? Dosyaya Kaydet",
            Location = new Point(485, 220),
            Size = new Size(445, 30),
            Enabled = false,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnKaydet.Click += BtnKaydet_Click;

        groupBox3.Controls.AddRange([txtLisansBilgi, txtLisansAnahtari, btnKopyala, btnKaydet]);
        tabYeniLisans.Controls.Add(groupBox3);
    }

    private void InitializeValidationTab()
    {
        groupBox4 = new GroupBox
        {
            Name = "groupBox4",
            Text = "Lisans Anahtarý",
            Location = new Point(20, 20),
            Size = new Size(950, 150),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        label6 = CreateLabel("label6", "Doðrulanacak Lisans Anahtarýný Girin:", new Point(20, 30));

        txtDogrulaAnahtar = new TextBox
        {
            Name = "txtDogrulaAnahtar",
            Location = new Point(20, 55),
            Size = new Size(910, 75),
            Font = new Font("Consolas", 9F),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        groupBox4.Controls.AddRange([label6, txtDogrulaAnahtar]);

        btnDogrula = new Button
        {
            Name = "btnDogrula",
            Text = "? LÝSANSI DOÐRULA",
            Location = new Point(20, 190),
            Size = new Size(950, 50),
            BackColor = Color.FromArgb(0, 123, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        btnDogrula.Click += BtnDogrula_Click;

        groupBox5 = new GroupBox
        {
            Name = "groupBox5",
            Text = "Doðrulama Sonucu",
            Location = new Point(20, 260),
            Size = new Size(950, 390),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        txtDogrulaSonuc = new TextBox
        {
            Name = "txtDogrulaSonuc",
            Location = new Point(20, 30),
            Size = new Size(910, 340),
            Font = new Font("Consolas", 9F),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        groupBox5.Controls.Add(txtDogrulaSonuc);
        tabDogrula.Controls.AddRange([groupBox4, btnDogrula, groupBox5]);
    }

    private void InitializeStatusBar()
    {
        statusStrip1 = new StatusStrip
        {
            Name = "statusStrip1"
        };

        toolStripStatusLabel1 = new ToolStripStatusLabel
        {
            Name = "toolStripStatusLabel1",
            Text = "CRM Filo Servis - Lisans Oluþturucu v3.0"
        };

        statusStrip1.Items.Add(toolStripStatusLabel1);
    }

    private void ConfigureFormLayout()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 700);
        MinimumSize = new Size(800, 600);
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CRM Filo Servis - Lisans Oluþturucu";

        Controls.AddRange([tabControl1, statusStrip1]);

        ResumeLayout(false);
        PerformLayout();
    }

    #region Helper Methods

    private static RadioButton CreateRadioButton(string name, string text, Point location, int tabIndex, bool isChecked = false)
    {
        return new RadioButton
        {
            Name = name,
            Text = text,
            Location = location,
            AutoSize = true,
            TabIndex = tabIndex,
            Checked = isChecked,
            UseVisualStyleBackColor = true
        };
    }

    private static Label CreateLabel(string name, string text, Point location)
    {
        return new Label
        {
            Name = name,
            Text = text,
            Location = location,
            AutoSize = true
        };
    }

    private static TextBox CreateTextBox(string name, Point location, int width)
    {
        return new TextBox
        {
            Name = name,
            Location = location,
            Size = new Size(width, 23),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
    }

    private static NumericUpDown CreateNumericUpDown(string name, Point location, int maxValue, int defaultValue, int minValue = 0, int width = 80)
    {
        return new NumericUpDown
        {
            Name = name,
            Location = location,
            Size = new Size(width, 23),
            Maximum = maxValue,
            Minimum = minValue,
            Value = defaultValue
        };
    }

    #endregion

    #region Event Handler Stubs (implement business logic here or in a separate partial class file)

    private void BtnMakineKoduAl_Click(object? sender, EventArgs e)
    {
        // TODO: Implement machine code retrieval logic
    }

    private void BtnOlustur_Click(object? sender, EventArgs e)
    {
        // TODO: Implement license generation logic
    }

    private void BtnKopyala_Click(object? sender, EventArgs e)
    {
        // TODO: Implement copy to clipboard logic
        if (!string.IsNullOrEmpty(txtLisansAnahtari.Text))
        {
            Clipboard.SetText(txtLisansAnahtari.Text);
            MessageBox.Show("Lisans anahtarý panoya kopyalandý!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnKaydet_Click(object? sender, EventArgs e)
    {
        // TODO: Implement save to file logic
    }

    private void BtnDogrula_Click(object? sender, EventArgs e)
    {
        // TODO: Implement license validation logic
    }

    #endregion
}

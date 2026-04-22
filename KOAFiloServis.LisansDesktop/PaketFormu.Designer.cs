namespace KOAFiloServis.LisansDesktop;

partial class PaketFormu
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblBaslik = new Label();
        lblWorkspace = new Label();
        txtWorkspace = new TextBox();
        btnWorkspaceSec = new Button();
        lblOutputDir = new Label();
        txtOutputDir = new TextBox();
        btnOutputDirSec = new Button();
        chkSkipBuild = new CheckBox();
        btnUpdate = new Button();
        btnInstall = new Button();
        btnCiktiAc = new Button();
        lblDurum = new Label();
        lblCikti = new Label();
        txtCikti = new TextBox();
        SuspendLayout();

        // lblBaslik
        lblBaslik.AutoSize = true;
        lblBaslik.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblBaslik.Location = new Point(20, 15);
        lblBaslik.Text = "IIS Kurulum / Guncelleme Paketi Olusturucu";

        // lblWorkspace
        lblWorkspace.AutoSize = true;
        lblWorkspace.Location = new Point(22, 70);
        lblWorkspace.Text = "Workspace klasoru:";

        // txtWorkspace
        txtWorkspace.Location = new Point(170, 67);
        txtWorkspace.Size = new Size(560, 27);
        txtWorkspace.TextChanged += txtWorkspace_TextChanged;

        // btnWorkspaceSec
        btnWorkspaceSec.Location = new Point(740, 65);
        btnWorkspaceSec.Size = new Size(110, 31);
        btnWorkspaceSec.Text = "Sec...";
        btnWorkspaceSec.UseVisualStyleBackColor = true;
        btnWorkspaceSec.Click += btnWorkspaceSec_Click;

        // chkSkipBuild
        chkSkipBuild.AutoSize = true;
        chkSkipBuild.Location = new Point(170, 105);
        chkSkipBuild.Text = "Build atla (publish\\IIS klasoru zaten hazirsa)";
        chkSkipBuild.UseVisualStyleBackColor = true;

        // btnUpdate
        btnUpdate.Location = new Point(22, 175);
        btnUpdate.Size = new Size(280, 80);
        btnUpdate.Text = "GUNCELLEME PAKETI\r\n(Mevcut DB korunur)";
        btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnUpdate.BackColor = Color.LightSteelBlue;
        btnUpdate.UseVisualStyleBackColor = false;
        btnUpdate.Click += btnUpdate_Click;

        // btnInstall
        btnInstall.Location = new Point(320, 175);
        btnInstall.Size = new Size(280, 80);
        btnInstall.Text = "YENI KURULUM PAKETI\r\n(DB SIFIRLANIR)";
        btnInstall.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnInstall.BackColor = Color.LightSalmon;
        btnInstall.UseVisualStyleBackColor = false;
        btnInstall.Click += btnInstall_Click;

        // btnCiktiAc
        btnCiktiAc.Location = new Point(620, 175);
        btnCiktiAc.Size = new Size(230, 80);
        btnCiktiAc.Text = "publish klasorunu ac";
        btnCiktiAc.UseVisualStyleBackColor = true;
        btnCiktiAc.Click += btnCiktiAc_Click;

        // lblDurum
        lblDurum.AutoSize = true;
        lblDurum.Location = new Point(22, 265);
        lblDurum.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        // lblCikti
        lblCikti.AutoSize = true;
        lblCikti.Location = new Point(22, 295);
        lblCikti.Text = "Cikti:";

        // txtCikti
        txtCikti.Location = new Point(22, 320);
        txtCikti.Size = new Size(828, 280);
        txtCikti.Multiline = true;
        txtCikti.ScrollBars = ScrollBars.Vertical;
        txtCikti.ReadOnly = true;
        txtCikti.Font = new Font("Consolas", 9F);
        txtCikti.BackColor = Color.Black;
        txtCikti.ForeColor = Color.LightGreen;

        // PaketFormu
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(880, 620);
        Controls.Add(lblBaslik);
        Controls.Add(lblWorkspace);
        Controls.Add(txtWorkspace);
        Controls.Add(btnWorkspaceSec);
        Controls.Add(lblOutputDir);
        Controls.Add(txtOutputDir);
        Controls.Add(btnOutputDirSec);
        Controls.Add(chkSkipBuild);
        Controls.Add(btnUpdate);
        Controls.Add(btnInstall);
        Controls.Add(btnCiktiAc);
        Controls.Add(lblDurum);
        Controls.Add(lblCikti);
        Controls.Add(txtCikti);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "IIS Paketi Olusturucu";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblBaslik;
    private Label lblWorkspace;
    private TextBox txtWorkspace;
    private Button btnWorkspaceSec;
    private Label lblOutputDir;
    private TextBox txtOutputDir;
    private Button btnOutputDirSec;
    private CheckBox chkSkipBuild;
    private Button btnUpdate;
    private Button btnInstall;
    private Button btnCiktiAc;
    private Label lblDurum;
    private Label lblCikti;
    private TextBox txtCikti;
}



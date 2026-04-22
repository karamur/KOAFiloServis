using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace KOAFiloServis.LisansDesktop;

public partial class PaketFormu : Form
{
    private readonly string _ayarYolu;
    private PaketAyarlari _ayarlar;
    private Process? _aktifProcess;

    public PaketFormu()
    {
        InitializeComponent();

        var klasor = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KOAFiloServis", "LisansOlusturucu");
        Directory.CreateDirectory(klasor);
        _ayarYolu = Path.Combine(klasor, "paket_ayarlari.json");

        _ayarlar = AyarlariYukle();
        if (string.IsNullOrWhiteSpace(_ayarlar.WorkspacePath))
        {
            _ayarlar.WorkspacePath = WorkspaceTahminEt();
        }
        if (string.IsNullOrWhiteSpace(_ayarlar.OutputDir) && !string.IsNullOrWhiteSpace(_ayarlar.WorkspacePath))
        {
            _ayarlar.OutputDir = Path.Combine(_ayarlar.WorkspacePath, "publish");
        }
        txtWorkspace.Text = _ayarlar.WorkspacePath ?? string.Empty;
        txtOutputDir.Text = _ayarlar.OutputDir ?? string.Empty;
        chkSkipBuild.Checked = _ayarlar.SkipBuild;
        DurumGuncelle();
    }

    private static string WorkspaceTahminEt()
    {
        // exe konumundan yukari dogru paketle.ps1 ara
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && dir != null; i++)
        {
            var probe = Path.Combine(dir, "scripts", "paketle.ps1");
            if (File.Exists(probe))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return string.Empty;
    }

    private PaketAyarlari AyarlariYukle()
    {
        try
        {
            if (File.Exists(_ayarYolu))
            {
                var json = File.ReadAllText(_ayarYolu, Encoding.UTF8);
                return JsonSerializer.Deserialize<PaketAyarlari>(json) ?? new PaketAyarlari();
            }
        }
        catch
        {
            // ignore
        }
        return new PaketAyarlari();
    }

    private void AyarlariKaydet()
    {
        try
        {
            _ayarlar.WorkspacePath = txtWorkspace.Text.Trim();
            _ayarlar.OutputDir = txtOutputDir.Text.Trim();
            _ayarlar.SkipBuild = chkSkipBuild.Checked;
            var json = JsonSerializer.Serialize(_ayarlar, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_ayarYolu, json, Encoding.UTF8);
        }
        catch
        {
            // ignore
        }
    }

    private void btnWorkspaceSec_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Workspace klasorunu secin (paketle.ps1 dosyasini iceren proje koku)",
            UseDescriptionForTitle = true,
            SelectedPath = txtWorkspace.Text
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtWorkspace.Text = dlg.SelectedPath;
            if (string.IsNullOrWhiteSpace(txtOutputDir.Text))
            {
                txtOutputDir.Text = Path.Combine(dlg.SelectedPath, "publish");
            }
            DurumGuncelle();
        }
    }

    private void btnOutputDirSec_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Setup dosyalarinin yazilacagi cikti klasorunu secin",
            UseDescriptionForTitle = true,
            SelectedPath = txtOutputDir.Text
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtOutputDir.Text = dlg.SelectedPath;
            DurumGuncelle();
        }
    }

    private void btnUpdate_Click(object? sender, EventArgs e) => PaketUret("Update");
    private void btnInstall_Click(object? sender, EventArgs e)
    {
        var sonuc = MessageBox.Show(
            "YENI KURULUM paketi olusturulacak. Bu paket hedefte calistiginda:\n\n" +
            " • Mevcut dbsettings.json paket icindekiyle DEGISTIRILIR\n" +
            " • SQLite veritabani SIFIRLANIR (yedeklendikten sonra)\n\n" +
            "Devam edilsin mi?",
            "Yeni Kurulum Paketi",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (sonuc != DialogResult.Yes) return;
        PaketUret("Install");
    }

    private void PaketUret(string mode)
    {
        if (!WorkspaceGecerli())
        {
            MessageBox.Show("Once gecerli bir workspace klasoru secin (scripts\\paketle.ps1 dosyasini iceren).",
                "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtOutputDir.Text))
        {
            MessageBox.Show("Once setup cikti klasoru secin.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Directory.CreateDirectory(txtOutputDir.Text.Trim());
        AyarlariKaydet();

        txtCikti.Clear();
        AppendLine($"=== Paket olusturma basliyor: {mode} ===");
        AppendLine($"Workspace : {txtWorkspace.Text}");
        AppendLine($"OutputDir : {txtOutputDir.Text}");
        AppendLine($"SkipBuild : {chkSkipBuild.Checked}");
        AppendLine("");

        ButonlariAyarla(false);

        var args = new StringBuilder();
        args.Append("-NoProfile -ExecutionPolicy Bypass -File \"");
        args.Append(Path.Combine(txtWorkspace.Text, "scripts", "paketle.ps1"));
        args.Append("\" -Mode ").Append(mode);
        args.Append(" -OutputDir \"").Append(txtOutputDir.Text.Trim()).Append("\"");
        if (chkSkipBuild.Checked) args.Append(" -SkipBuild");

        var psi = new ProcessStartInfo
        {
            FileName = "pwsh.exe",
            Arguments = args.ToString(),
            WorkingDirectory = txtWorkspace.Text,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {
            _aktifProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _aktifProcess.OutputDataReceived += (_, ev) => { if (ev.Data != null) AppendLine(ev.Data); };
            _aktifProcess.ErrorDataReceived  += (_, ev) => { if (ev.Data != null) AppendLine("[HATA] " + ev.Data); };
            _aktifProcess.Exited += (_, _) =>
            {
                var rc = _aktifProcess?.ExitCode ?? -1;
                BeginInvoke(new Action(() =>
                {
                    AppendLine("");
                    AppendLine(rc == 0
                        ? $"=== Paket olusturma TAMAMLANDI ({mode}) ==="
                        : $"=== Paket olusturma HATA ile bitti. ExitCode={rc} ===");
                    ButonlariAyarla(true);
                    if (rc == 0) btnCiktiAc.Enabled = true;
                    _aktifProcess?.Dispose();
                    _aktifProcess = null;
                }));
            };
            _aktifProcess.Start();
            _aktifProcess.BeginOutputReadLine();
            _aktifProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            AppendLine("[HATA] " + ex.Message);
            ButonlariAyarla(true);
        }
    }

    private void btnCiktiAc_Click(object? sender, EventArgs e)
    {
        var path = txtOutputDir.Text.Trim();
        if (!Directory.Exists(path))
        {
            MessageBox.Show("Cikti klasoru bulunamadi: " + path, "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    }

    private void AppendLine(string s)
    {
        if (txtCikti.InvokeRequired)
        {
            txtCikti.BeginInvoke(new Action<string>(AppendLine), s);
            return;
        }
        txtCikti.AppendText(s + Environment.NewLine);
    }

    private void ButonlariAyarla(bool aktif)
    {
        btnUpdate.Enabled = aktif;
        btnInstall.Enabled = aktif;
        btnWorkspaceSec.Enabled = aktif;
        btnOutputDirSec.Enabled = aktif;
        txtOutputDir.Enabled = aktif;
        chkSkipBuild.Enabled = aktif;
        btnCiktiAc.Enabled = aktif && Directory.Exists(txtOutputDir.Text.Trim());
    }

    private bool WorkspaceGecerli()
    {
        var path = txtWorkspace.Text?.Trim();
        return !string.IsNullOrWhiteSpace(path)
               && Directory.Exists(path)
               && File.Exists(Path.Combine(path, "scripts", "paketle.ps1"));
    }

    private void DurumGuncelle()
    {
        var ok = WorkspaceGecerli();
        lblDurum.Text = ok
            ? "Workspace OK. Paket olusturmaya hazir."
            : "Workspace gecersiz. paketle.ps1 bulunamadi.";
        lblDurum.ForeColor = ok ? Color.DarkGreen : Color.Firebrick;
        btnUpdate.Enabled = ok;
        btnInstall.Enabled = ok;
        btnCiktiAc.Enabled = ok && Directory.Exists(txtOutputDir.Text.Trim());
    }

    private void txtWorkspace_TextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtOutputDir.Text) && Directory.Exists(txtWorkspace.Text.Trim()))
        {
            txtOutputDir.Text = Path.Combine(txtWorkspace.Text.Trim(), "publish");
        }
        DurumGuncelle();
    }

    private void txtOutputDir_TextChanged(object? sender, EventArgs e) => DurumGuncelle();

    private sealed class PaketAyarlari
    {
        public string? WorkspacePath { get; set; }
        public string? OutputDir { get; set; }
        public bool SkipBuild { get; set; }
    }
}



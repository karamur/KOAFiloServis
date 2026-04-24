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
            _ayarlar.OutputDir = Path.Combine(_ayarlar.WorkspacePath, "setup", "output");
        }
        txtWorkspace.Text = _ayarlar.WorkspacePath ?? string.Empty;
        txtOutputDir.Text = _ayarlar.OutputDir ?? string.Empty;
        txtVersiyon.Text = _ayarlar.Versiyon ?? string.Empty;
        chkSkipBuild.Checked = _ayarlar.SkipBuild;
        DurumGuncelle();
    }

    private static string WorkspaceTahminEt()
    {
        // exe konumundan yukari dogru once yeni setup akisini, sonra legacy scripti ara
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && dir != null; i++)
        {
            var setupBuild = Path.Combine(dir, "setup", "build.ps1");
            if (File.Exists(setupBuild))
            {
                return dir;
            }
            var legacyPaketle = Path.Combine(dir, "scripts", "paketle.ps1");
            if (File.Exists(legacyPaketle))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return string.Empty;
    }

    private string? GetPaketScriptPath()
    {
        var root = txtWorkspace.Text?.Trim();
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            return null;
        }

        var setupBuild = Path.Combine(root, "setup", "build.ps1");
        if (File.Exists(setupBuild))
        {
            return setupBuild;
        }

        var legacyPaketle = Path.Combine(root, "scripts", "paketle.ps1");
        if (File.Exists(legacyPaketle))
        {
            return legacyPaketle;
        }

        return null;
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
            _ayarlar.Versiyon = txtVersiyon.Text.Trim();
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
            Description = "Workspace klasorunu secin (setup\\build.ps1 veya scripts\\paketle.ps1 iceren proje koku)",
            UseDescriptionForTitle = true,
            SelectedPath = txtWorkspace.Text
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtWorkspace.Text = dlg.SelectedPath;
            if (string.IsNullOrWhiteSpace(txtOutputDir.Text))
            {
                txtOutputDir.Text = Path.Combine(dlg.SelectedPath, "setup", "output");
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

    private void btnUpdate_Click(object? sender, EventArgs e) => PaketUretAll();
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
        PaketUretAll();
    }

    private void PaketUretAll()
    {
        var paketScript = GetPaketScriptPath();
        if (string.IsNullOrWhiteSpace(paketScript))
        {
            MessageBox.Show("Once gecerli bir workspace klasoru secin (setup\\build.ps1 veya scripts\\paketle.ps1 dosyasini iceren).",
                "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtOutputDir.Text))
        {
            MessageBox.Show("Once setup cikti klasoru secin.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var versiyon = (txtVersiyon.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(versiyon))
        {
            MessageBox.Show("Once versiyon girin (orn: 1.2.3).", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtVersiyon.Focus();
            return;
        }

        // Gecersiz dosya yolu karakterlerini ayikla
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            versiyon = versiyon.Replace(c.ToString(), string.Empty);
        }

        var workspaceRoot = txtWorkspace.Text.Trim();
        var outputRoot = txtOutputDir.Text.Trim();
        var isSetupBuild = paketScript.EndsWith(Path.Combine("setup", "build.ps1"), StringComparison.OrdinalIgnoreCase);

        // setup/build.ps1 ciktiyi sabit olarak workspace\setup\output\v<versiyon> altina yazar.
        var varsayilanSetupOutputRoot = Path.Combine(workspaceRoot, "setup", "output");
        var versiyonluOutputDir = isSetupBuild
            ? Path.Combine(varsayilanSetupOutputRoot, "v" + versiyon)
            : Path.Combine(outputRoot, versiyon);

        Directory.CreateDirectory(versiyonluOutputDir);
        AyarlariKaydet();

        txtCikti.Clear();
        AppendLine("=== Paket olusturma basliyor: All (Update + Install) ===");
        AppendLine($"Workspace : {txtWorkspace.Text}");
        AppendLine($"OutputDir : {versiyonluOutputDir}");
        AppendLine($"Versiyon  : {versiyon}");
        if (isSetupBuild)
        {
            AppendLine($"SkipBuild : {chkSkipBuild.Checked} (build.ps1 icin SkipPublish olarak kullanilir)");
        }
        else
        {
            AppendLine($"SkipBuild : {chkSkipBuild.Checked} (All modunda sadece Update adiminda kullanilir)");
        }
        AppendLine("");

        ButonlariAyarla(false);

        var args = new StringBuilder();
        args.Append("-NoProfile -ExecutionPolicy Bypass -File \"");
        args.Append(paketScript);
        args.Append("\"");

        if (isSetupBuild)
        {
            args.Append(" -Version \"").Append(versiyon).Append("\"");
            if (chkSkipBuild.Checked) args.Append(" -SkipPublish");
            if (!string.Equals(outputRoot, varsayilanSetupOutputRoot, StringComparison.OrdinalIgnoreCase))
            {
                AppendLine("[BILGI] setup/build.ps1 secili oldugu icin OutputDir ayari yok sayildi.");
                AppendLine("[BILGI] Cikti varsayilan klasore yazilacak: " + varsayilanSetupOutputRoot);
                AppendLine("");
            }
        }
        else
        {
            args.Append(" -Mode All");
            args.Append(" -OutputDir \"").Append(versiyonluOutputDir).Append("\"");
            args.Append(" -Version \"").Append(versiyon).Append("\"");
            if (chkSkipBuild.Checked) args.Append(" -SkipBuild");
        }

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
                        ? "=== Paket olusturma TAMAMLANDI (Update + Install) ==="
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
        var root = txtOutputDir.Text.Trim();
        var versiyon = (txtVersiyon.Text ?? string.Empty).Trim();
        var path = !string.IsNullOrWhiteSpace(versiyon)
            ? Path.Combine(root, versiyon)
            : root;
        if (!Directory.Exists(path))
        {
            // Versiyonlu klasor yoksa kok klasore d\u00FCs
            if (!Directory.Exists(root))
            {
                MessageBox.Show("Cikti klasoru bulunamadi: " + path, "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            path = root;
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
        return !string.IsNullOrWhiteSpace(GetPaketScriptPath());
    }

    private void DurumGuncelle()
    {
        var ok = WorkspaceGecerli();
        lblDurum.Text = ok
            ? "Workspace OK. Paket olusturmaya hazir."
            : "Workspace gecersiz. setup\\build.ps1 veya scripts\\paketle.ps1 bulunamadi.";
        lblDurum.ForeColor = ok ? Color.DarkGreen : Color.Firebrick;
        btnUpdate.Enabled = ok;
        btnInstall.Enabled = ok;
        btnCiktiAc.Enabled = ok && Directory.Exists(txtOutputDir.Text.Trim());
    }

    private void txtWorkspace_TextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtOutputDir.Text) && Directory.Exists(txtWorkspace.Text.Trim()))
        {
            txtOutputDir.Text = Path.Combine(txtWorkspace.Text.Trim(), "setup", "output");
        }
        DurumGuncelle();
    }

    private void txtOutputDir_TextChanged(object? sender, EventArgs e) => DurumGuncelle();

    private sealed class PaketAyarlari
    {
        public string? WorkspacePath { get; set; }
        public string? OutputDir { get; set; }
        public string? Versiyon { get; set; }
        public bool SkipBuild { get; set; }
    }
}





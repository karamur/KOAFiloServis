using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using CRMFiloServis.Shared;
using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Installer;

public partial class MainForm : Form
{
    private enum KurulumTipi { Normal, Docker }
    private enum KurulumModu { YeniKurulum, MevcutYedekIle }

    private KurulumTipi _kurulumTipi = KurulumTipi.Normal;
    private KurulumModu _kurulumModu = KurulumModu.YeniKurulum;
    private string _hedefDizin = @"C:\KOAFiloServis";
    private string? _yedekDosyaYolu;
    private string? _paketDosyaYolu;

    public MainForm()
    {
        InitializeComponent();
        txtMakineKodu.Text = LisansHelper.GetMachineCode();
    }

    private void rbNormal_CheckedChanged(object sender, EventArgs e)
    {
        if (rbNormal.Checked)
        {
            _kurulumTipi = KurulumTipi.Normal;
            pnlDockerAyarlari.Visible = false;
            pnlNormalAyarlari.Visible = true;
        }
    }

    private void rbDocker_CheckedChanged(object sender, EventArgs e)
    {
        if (rbDocker.Checked)
        {
            _kurulumTipi = KurulumTipi.Docker;
            pnlDockerAyarlari.Visible = true;
            pnlNormalAyarlari.Visible = false;
        }
    }

    private void rbYeniKurulum_CheckedChanged(object sender, EventArgs e)
    {
        if (rbYeniKurulum.Checked)
        {
            _kurulumModu = KurulumModu.YeniKurulum;
            grpYedek.Enabled = false;
            lblYedekDurum.Text = "Sıfır veritabanı ile kurulum yapılacak.";
        }
    }

    private void rbMevcutYedek_CheckedChanged(object sender, EventArgs e)
    {
        if (rbMevcutYedek.Checked)
        {
            _kurulumModu = KurulumModu.MevcutYedekIle;
            grpYedek.Enabled = true;
            lblYedekDurum.Text = "Yedek dosyası seçilmedi.";
        }
    }

    private void btnYedekSec_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Veritabanı Yedek Dosyası Seçin",
            Filter = "Yedek Dosyaları|*.backup;*.sql;*.bak;*.zip|Tüm Dosyalar|*.*"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _yedekDosyaYolu = dlg.FileName;
            lblYedekDurum.Text = $"Seçilen: {Path.GetFileName(_yedekDosyaYolu)}";
        }
    }

    private void btnPaketSec_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Kurulum Paketi Seçin",
            Filter = "ZIP Dosyaları|*.zip"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _paketDosyaYolu = dlg.FileName;
            lblPaketDurum.Text = $"Seçilen: {Path.GetFileName(_paketDosyaYolu)}";
        }
    }

    private void btnDizinSec_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Kurulum Dizini Seçin",
            SelectedPath = _hedefDizin
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _hedefDizin = dlg.SelectedPath;
            txtHedefDizin.Text = _hedefDizin;
        }
    }

    private void btnMakineKoduKopyala_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtMakineKodu.Text))
        {
            Clipboard.SetText(txtMakineKodu.Text);
            MessageBox.Show("Makine kodu panoya kopyalandı.\n\nBu kodu lisans almak için kullanın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void btnKurulumBaslat_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_paketDosyaYolu) || !File.Exists(_paketDosyaYolu))
        {
            MessageBox.Show("Lütfen önce kurulum paketini seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_kurulumModu == KurulumModu.MevcutYedekIle && (string.IsNullOrWhiteSpace(_yedekDosyaYolu) || !File.Exists(_yedekDosyaYolu)))
        {
            MessageBox.Show("Mevcut yedek ile kurulum seçtiniz ancak yedek dosyası belirtmediniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var onay = MessageBox.Show(
            $"Kurulum Tipi: {(_kurulumTipi == KurulumTipi.Normal ? "Normal (IIS/Windows)" : "Docker")}\n" +
            $"Kurulum Modu: {(_kurulumModu == KurulumModu.YeniKurulum ? "Sıfır Kurulum" : "Mevcut Yedek İle")}\n" +
            $"Hedef Dizin: {_hedefDizin}\n\n" +
            "Kurulumu başlatmak istiyor musunuz?",
            "Kurulum Onayı",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (onay != DialogResult.Yes) return;

        btnKurulumBaslat.Enabled = false;
        progressBar.Visible = true;
        lblDurum.Visible = true;

        try
        {
            if (_kurulumTipi == KurulumTipi.Normal)
            {
                await NormalKurulumAsync();
            }
            else
            {
                await DockerKurulumAsync();
            }

            MessageBox.Show("Kurulum başarıyla tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kurulum hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnKurulumBaslat.Enabled = true;
            progressBar.Visible = false;
            lblDurum.Visible = false;
        }
    }

    private async Task NormalKurulumAsync()
    {
        lblDurum.Text = "Dizin oluşturuluyor...";
        progressBar.Value = 10;
        await Task.Delay(100);

        Directory.CreateDirectory(_hedefDizin);

        lblDurum.Text = "Paket açılıyor...";
        progressBar.Value = 30;
        await Task.Delay(100);

        var tempDir = Path.Combine(Path.GetTempPath(), "CRMFiloServis_Install_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        ZipFile.ExtractToDirectory(_paketDosyaYolu!, tempDir, overwriteFiles: true);

        var sourceDir = tempDir;
        var publishSubDir = Path.Combine(tempDir, "publish");
        if (Directory.Exists(publishSubDir))
            sourceDir = publishSubDir;

        lblDurum.Text = "Dosyalar kopyalanıyor...";
        progressBar.Value = 50;
        await Task.Delay(100);

        CopyDirectory(sourceDir, _hedefDizin);

        if (_kurulumModu == KurulumModu.MevcutYedekIle && !string.IsNullOrWhiteSpace(_yedekDosyaYolu))
        {
            lblDurum.Text = "Yedek dosyası kopyalanıyor...";
            progressBar.Value = 70;
            await Task.Delay(100);

            var yedekHedef = Path.Combine(_hedefDizin, "Data", "restore");
            Directory.CreateDirectory(yedekHedef);
            File.Copy(_yedekDosyaYolu, Path.Combine(yedekHedef, Path.GetFileName(_yedekDosyaYolu)), overwrite: true);
        }

        lblDurum.Text = "Yapılandırma oluşturuluyor...";
        progressBar.Value = 85;
        await Task.Delay(100);

        var prodConfig = Path.Combine(_hedefDizin, "appsettings.Production.json");
        var exampleConfig = Path.Combine(_hedefDizin, "appsettings.Production.json.example");
        if (!File.Exists(prodConfig) && File.Exists(exampleConfig))
        {
            File.Copy(exampleConfig, prodConfig);
        }

        ConfigureSqliteForNormalInstall();
        CreateDesktopShortcut();

        try { Directory.Delete(tempDir, true); } catch { }

        lblDurum.Text = "Kurulum tamamlandı.";
        progressBar.Value = 100;
    }

    private async Task DockerKurulumAsync()
    {
        lblDurum.Text = "Docker dizini oluşturuluyor...";
        progressBar.Value = 10;
        await Task.Delay(100);

        var dockerDir = Path.Combine(_hedefDizin, "docker");
        Directory.CreateDirectory(dockerDir);

        lblDurum.Text = "Paket açılıyor...";
        progressBar.Value = 30;
        await Task.Delay(100);

        var tempDir = Path.Combine(Path.GetTempPath(), "CRMFiloServis_Install_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        ZipFile.ExtractToDirectory(_paketDosyaYolu!, tempDir, overwriteFiles: true);

        var sourceDir = tempDir;
        var publishSubDir = Path.Combine(tempDir, "publish");
        if (Directory.Exists(publishSubDir))
            sourceDir = publishSubDir;

        lblDurum.Text = "Dosyalar kopyalanıyor...";
        progressBar.Value = 50;
        await Task.Delay(100);

        CopyDirectory(sourceDir, dockerDir);

        if (_kurulumModu == KurulumModu.MevcutYedekIle && !string.IsNullOrWhiteSpace(_yedekDosyaYolu))
        {
            lblDurum.Text = "Yedek dosyası kopyalanıyor...";
            progressBar.Value = 65;
            await Task.Delay(100);

            var yedekHedef = Path.Combine(dockerDir, "restore");
            Directory.CreateDirectory(yedekHedef);
            File.Copy(_yedekDosyaYolu, Path.Combine(yedekHedef, Path.GetFileName(_yedekDosyaYolu)), overwrite: true);
        }

        lblDurum.Text = "Docker dosyaları oluşturuluyor...";
        progressBar.Value = 80;
        await Task.Delay(100);

        var dockerfile = Path.Combine(dockerDir, "Dockerfile");
        var compose = Path.Combine(dockerDir, "docker-compose.yml");
        var envFile = Path.Combine(dockerDir, ".env");

        File.WriteAllText(dockerfile, GetDockerfileContent());
        File.WriteAllText(compose, GetDockerComposeContent());
        File.WriteAllText(envFile, GetEnvContent());

        try { Directory.Delete(tempDir, true); } catch { }

        lblDurum.Text = "Docker kurulumu tamamlandı.";
        progressBar.Value = 100;

        MessageBox.Show(
            $"Docker dosyaları oluşturuldu:\n{dockerDir}\n\n" +
            "Başlatmak için:\n" +
            "cd " + dockerDir + "\n" +
            "docker compose up -d --build",
            "Docker Kurulum",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }

    private void ConfigureSqliteForNormalInstall()
    {
        var storageRoot = @"C:\KOAFiloServis_yedekleme";
        var sqliteRelativePath = "CRMFiloServis.db";
        var sqliteFullPath = Path.Combine(_hedefDizin, sqliteRelativePath);

        Directory.CreateDirectory(_hedefDizin);
        Directory.CreateDirectory(storageRoot);
        Directory.CreateDirectory(Path.Combine(storageRoot, "database"));
        Directory.CreateDirectory(Path.Combine(storageRoot, "uploads"));
        Directory.CreateDirectory(Path.Combine(storageRoot, "keys"));
        Directory.CreateDirectory(Path.Combine(storageRoot, "logs"));

        if (!File.Exists(sqliteFullPath))
        {
            using var stream = File.Create(sqliteFullPath);
        }

        var prodJson = """
        {
          "DatabaseProvider": "SQLite",
          "ConnectionStrings": {
            "DefaultConnection": "Data Source=CRMFiloServis.db;"
          },
          "OpenAI": {
            "ApiKey": "",
            "Model": "gpt-4o-mini",
            "BaseUrl": "https://api.openai.com/v1"
          },
          "PythonScraper": {
            "BaseUrl": "http://localhost:5050",
            "Enabled": false
          },
          "Logging": {
            "LogLevel": {
              "Default": "Information",
              "Microsoft.AspNetCore": "Warning",
              "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
              "Microsoft.EntityFrameworkCore.Infrastructure": "Warning",
              "System.Net.Http.HttpClient": "Warning"
            }
          },
          "AllowedHosts": "*"
        }
        """;

        File.WriteAllText(Path.Combine(_hedefDizin, "appsettings.Production.json"), prodJson);

        var dbSettings = new DatabaseSettings
        {
            Provider = DatabaseProvider.SQLite,
            DatabaseName = sqliteRelativePath,
            Host = string.Empty,
            Port = 0,
            Username = string.Empty,
            Password = string.Empty,
            UseIntegratedSecurity = false,
            AdditionalOptions = null,
            LastUpdated = DateTime.UtcNow
        };

        var dbSettingsJson = JsonSerializer.Serialize(dbSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(Path.Combine(_hedefDizin, "dbsettings.json"), dbSettingsJson);
    }

    private void CreateDesktopShortcut()
    {
        try
        {
            var exePath = Path.Combine(_hedefDizin, "CRMFiloServis.Web.exe");
            if (!File.Exists(exePath))
                return;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrWhiteSpace(desktopPath))
                return;

            var shortcutPath = Path.Combine(desktopPath, "CRMFiloServis.lnk");
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
                return;

            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = _hedefDizin;
            shortcut.IconLocation = exePath;
            shortcut.Description = "CRM Filo Servis";
            shortcut.Save();
        }
        catch
        {
        }
    }

    private static string GetDockerfileContent() => """
        FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
        WORKDIR /app
        COPY . .
        ENV ASPNETCORE_URLS=http://+:8080
        ENV ASPNETCORE_ENVIRONMENT=Production
        EXPOSE 8080
        ENTRYPOINT ["dotnet", "CRMFiloServis.Web.dll"]
        """;

    private static string GetDockerComposeContent() => """
        version: '3.9'
        services:
          crmfiloservis-web:
            build: .
            container_name: crmfiloservis-web
            restart: unless-stopped
            ports:
              - "8080:8080"
            env_file:
              - .env
            volumes:
              - crmfiloservis_uploads:/app/wwwroot/uploads
              - crmfiloservis_logs:/app/logs
            depends_on:
              - postgres

          postgres:
            image: postgres:16
            container_name: crmfiloservis-postgres
            restart: unless-stopped
            environment:
              POSTGRES_DB: CRMFiloServisDb
              POSTGRES_USER: postgres
              POSTGRES_PASSWORD: postgres
            ports:
              - "5432:5432"
            volumes:
              - crmfiloservis_pgdata:/var/lib/postgresql/data

        volumes:
          crmfiloservis_pgdata:
          crmfiloservis_uploads:
          crmfiloservis_logs:
        """;

    private static string GetEnvContent() => """
        ASPNETCORE_ENVIRONMENT=Production
        DatabaseProvider=PostgreSQL
        ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=CRMFiloServisDb;Username=postgres;Password=postgres
        PythonScraper__Enabled=false
        """;
}

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Npgsql;
using Spectre.Console;

namespace KOABackupTool;

class Program
{
    private static string _host = "localhost";
    private static int _port = 5432;
    private static string _database = "DestekCRMServisBlazorDb";
    private static string _username = "postgres";
    private static string _password = "Fast123";
    private static string _backupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KOA_Backups");

    private static readonly string ConfigFile = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory, 
        "backup-settings.json");

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Kayıtlı ayarları yükle
        LoadSettings();

        AnsiConsole.Write(new FigletText("KOA Backup").Centered().Color(Color.Blue));
        AnsiConsole.MarkupLine("[grey]PostgreSQL Yedekleme Araci v1.0[/]");
        if (File.Exists(ConfigFile))
            AnsiConsole.MarkupLine($"[grey]Ayarlar yuklendi: {_host}:{_port}/{_database}[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[green]Ne yapmak istiyorsunuz?[/]").PageSize(10)
                .AddChoices(new[] { "1. Yedek Al", "2. Yedek Yukle", "3. Ayarlar", "4. Klasor Ac", "5. Listele", "6. Cikis" }));
            switch (choice[0])
            {
                case '1': await CreateBackupAsync(); break;
                case '2': await RestoreBackupAsync(); break;
                case '3': ConfigureConnection(); break;
                case '4': OpenBackupFolder(); break;
                case '5': ListBackups(); break;
                case '6': return;
            }
            AnsiConsole.WriteLine();
        }
    }

    static void LoadSettings()
    {
        try
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                var settings = JsonSerializer.Deserialize<BackupSettings>(json);
                if (settings != null)
                {
                    _host = settings.Host ?? _host;
                    _port = settings.Port > 0 ? settings.Port : _port;
                    _database = settings.Database ?? _database;
                    _username = settings.Username ?? _username;
                    _password = settings.Password ?? _password;
                    _backupFolder = settings.BackupFolder ?? _backupFolder;
                }
            }
        }
        catch { /* İlk çalıştırma veya bozuk dosya */ }
    }

    static void SaveSettings()
    {
        try
        {
            var settings = new BackupSettings
            {
                Host = _host,
                Port = _port,
                Database = _database,
                Username = _username,
                Password = _password,
                BackupFolder = _backupFolder
            };
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
            AnsiConsole.MarkupLine($"[green]Ayarlar kaydedildi: {ConfigFile}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Ayarlar kaydedilemedi: {ex.Message}[/]");
        }
    }

    static void ConfigureConnection()
    {
        _host = AnsiConsole.Prompt(new TextPrompt<string>("[green]Host:[/]").DefaultValue(_host));
        _port = AnsiConsole.Prompt(new TextPrompt<int>("[green]Port:[/]").DefaultValue(_port));
        _database = AnsiConsole.Prompt(new TextPrompt<string>("[green]Database:[/]").DefaultValue(_database));
        _username = AnsiConsole.Prompt(new TextPrompt<string>("[green]User:[/]").DefaultValue(_username));
        _password = AnsiConsole.Prompt(new TextPrompt<string>("[green]Password:[/]").Secret('*'));
        _backupFolder = AnsiConsole.Prompt(new TextPrompt<string>("[green]Backup Folder:[/]").DefaultValue(_backupFolder));

        // Bağlantıyı test et
        try 
        { 
            using var c = new NpgsqlConnection(GetConnectionString()); 
            c.Open(); 
            AnsiConsole.MarkupLine("[green]Baglanti basarili![/]");

            // Ayarları kaydet
            SaveSettings();
        }
        catch (Exception ex) 
        { 
            AnsiConsole.MarkupLine("[red]Baglanti hatasi: " + ex.Message + "[/]"); 
        }
    }

    static string GetConnectionString() => $"Host={_host};Port={_port};Database={_database};Username={_username};Password={_password}";

    static async Task CreateBackupAsync()
    {
        if (string.IsNullOrEmpty(_password)) _password = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Password:[/]").Secret('*'));
        Directory.CreateDirectory(_backupFolder);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupFile = Path.Combine(_backupFolder, $"{_database}_backup_{timestamp}.sql");
        var compressedFile = backupFile + ".gz";

        try
        {
            await AnsiConsole.Progress().StartAsync(async ctx => {
                var task = ctx.AddTask("[green]Yedekleniyor[/]");
                using var conn = new NpgsqlConnection(GetConnectionString());
                await conn.OpenAsync();
                task.Increment(5);

                var sb = new StringBuilder();
                sb.AppendLine("-- KOA Filo Servis PostgreSQL Backup");
                sb.AppendLine("-- Database: " + _database);
                sb.AppendLine("-- Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("-- Tool: KOABackupTool v1.0");
                sb.AppendLine();
                sb.AppendLine("SET client_encoding = 'UTF8';");
                sb.AppendLine();

                // Önce __EFMigrationsHistory tablosunu oluştur ve yedekle
                sb.AppendLine("-- EF Core Migrations History");
                sb.AppendLine(@"CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
    ""MigrationId"" character varying(150) NOT NULL,
    ""ProductVersion"" character varying(32) NOT NULL,
    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
);");
                sb.AppendLine();

                // Migration verilerini yedekle
                using (var cmd = new NpgsqlCommand("SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"", conn))
                {
                    try
                    {
                        using var r = await cmd.ExecuteReaderAsync();
                        while (await r.ReadAsync())
                        {
                            sb.AppendLine($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{r.GetString(0)}', '{r.GetString(1)}') ON CONFLICT DO NOTHING;");
                        }
                    }
                    catch { /* Tablo yoksa devam et */ }
                }
                sb.AppendLine();
                task.Increment(5);

                // Diğer tabloları listele (__EFMigrationsHistory hariç)
                var tables = new List<string>();
                using (var cmd = new NpgsqlCommand("SELECT tablename FROM pg_tables WHERE schemaname='public' AND tablename != '__EFMigrationsHistory' ORDER BY tablename", conn))
                using (var r = await cmd.ExecuteReaderAsync()) while (await r.ReadAsync()) tables.Add(r.GetString(0));
                task.Increment(10);

                foreach (var table in tables)
                {
                    sb.AppendLine($"-- Table: {table}");
                    sb.AppendLine($"DROP TABLE IF EXISTS \"{table}\" CASCADE;");
                    using (var cmd = new NpgsqlCommand($"SELECT 'CREATE TABLE \"{table}\" (' || string_agg('\"' || a.attname || '\" ' || pg_catalog.format_type(a.atttypid, a.atttypmod), ', ' ORDER BY a.attnum) || ')' FROM pg_attribute a WHERE a.attrelid = '{table}'::regclass AND a.attnum > 0 AND NOT a.attisdropped", conn))
                    {
                        var createStmt = await cmd.ExecuteScalarAsync();
                        if (createStmt != null) sb.AppendLine(createStmt.ToString() + ";");
                    }
                    using (var cmd = new NpgsqlCommand($"SELECT * FROM \"{table}\"", conn))
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        var cols = Enumerable.Range(0, r.FieldCount).Select(i => $"\"{r.GetName(i)}\"").ToList();
                        while (await r.ReadAsync())
                        {
                            var vals = Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? "NULL" : FormatValue(r.GetValue(i))).ToList();
                            sb.AppendLine($"INSERT INTO \"{table}\" ({string.Join(",", cols)}) VALUES ({string.Join(",", vals)});");
                        }
                    }
                    sb.AppendLine();
                    task.Increment(70.0 / tables.Count);
                }

                await File.WriteAllTextAsync(backupFile, sb.ToString());
                await using (var fs = File.OpenRead(backupFile))
                await using (var cs = File.Create(compressedFile))
                await using (var gz = new GZipStream(cs, CompressionLevel.Optimal))
                    await fs.CopyToAsync(gz);
                File.Delete(backupFile);
                task.Increment(10);
            });
            AnsiConsole.MarkupLine("[green]Yedek olusturuldu: " + compressedFile + "[/]");
        }
        catch (Exception ex) { AnsiConsole.MarkupLine("[red]" + ex.Message + "[/]"); }
    }

    static async Task EnsureDatabaseExistsAsync()
    {
        // postgres veritabanına bağlan ve hedef veritabanını kontrol et/oluştur
        var masterConnStr = $"Host={_host};Port={_port};Database=postgres;Username={_username};Password={_password}";

        try
        {
            using var conn = new NpgsqlConnection(masterConnStr);
            await conn.OpenAsync();

            // Veritabanı var mı kontrol et
            using var checkCmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{_database}'", conn);
            var exists = await checkCmd.ExecuteScalarAsync();

            if (exists == null)
            {
                AnsiConsole.MarkupLine($"[yellow]Veritabani '{_database}' bulunamadi, olusturuluyor...[/]");
                using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{_database}\" ENCODING 'UTF8'", conn);
                await createCmd.ExecuteNonQueryAsync();
                AnsiConsole.MarkupLine($"[green]Veritabani '{_database}' olusturuldu![/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Veritabani kontrol hatasi: {ex.Message}[/]");
            throw;
        }
    }

    static async Task EnsureMigrationTableExistsAsync(NpgsqlConnection conn)
    {
        // EF Core migration tablosunu oluştur
        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            );";

        using var cmd = new NpgsqlCommand(createTableSql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    static async Task RestoreBackupAsync()
    {
        if (!Directory.Exists(_backupFolder)) { AnsiConsole.MarkupLine("[red]Klasor yok[/]"); return; }
        var files = Directory.GetFiles(_backupFolder, "*.sql.gz").Concat(Directory.GetFiles(_backupFolder, "*.sql")).OrderByDescending(File.GetCreationTime).ToList();
        if (!files.Any()) { AnsiConsole.MarkupLine("[yellow]Yedek yok[/]"); return; }

        var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[green]Sec:[/]").AddChoices(files.Select(Path.GetFileName).Append("Geri")));
        if (sel == "Geri") return;
        var file = files.First(f => Path.GetFileName(f) == sel);
        if (!AnsiConsole.Confirm("Devam?", false)) return;
        if (string.IsNullOrEmpty(_password)) _password = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Password:[/]").Secret('*'));

        try
        {
            // Veritabanı yoksa oluştur
            await EnsureDatabaseExistsAsync();

            string sql;
            if (file.EndsWith(".gz")) { await using var fs = File.OpenRead(file); await using var gz = new GZipStream(fs, CompressionMode.Decompress); using var sr = new StreamReader(gz); sql = await sr.ReadToEndAsync(); }
            else sql = await File.ReadAllTextAsync(file);

            using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            // Migration tablosunu oluştur
            await EnsureMigrationTableExistsAsync(conn);

            AnsiConsole.MarkupLine("[grey]SQL komutlari calistiriliyor...[/]");
            var statements = sql.Split(';').Where(s => !string.IsNullOrWhiteSpace(s) && !s.TrimStart().StartsWith("--")).ToList();
            int success = 0, failed = 0;

            foreach (var stmt in statements)
            {
                try 
                { 
                    using var cmd = new NpgsqlCommand(stmt.Trim(), conn); 
                    await cmd.ExecuteNonQueryAsync();
                    success++;
                } 
                catch { failed++; }
            }

            AnsiConsole.MarkupLine($"[green]Yuklendi! ({success} basarili, {failed} atlandi)[/]");
        }
        catch (Exception ex) { AnsiConsole.MarkupLine("[red]" + ex.Message + "[/]"); }
    }

    static void OpenBackupFolder() { Directory.CreateDirectory(_backupFolder); System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = _backupFolder, UseShellExecute = true }); }
    
    static void ListBackups()
    {
        if (!Directory.Exists(_backupFolder)) { AnsiConsole.MarkupLine("[yellow]Klasor yok[/]"); return; }
        var files = Directory.GetFiles(_backupFolder, "*.sql*").Select(f => new FileInfo(f)).OrderByDescending(f => f.CreationTime);
        var table = new Table().AddColumn("Dosya").AddColumn("Boyut").AddColumn("Tarih");
        foreach (var f in files) table.AddRow(f.Name, FormatSize(f.Length), f.CreationTime.ToString("dd.MM.yyyy HH:mm"));
        AnsiConsole.Write(table);
    }

    static string FormatValue(object v) => v switch { string s => $"'{s.Replace("'", "''")}'", DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss}'", bool b => b ? "TRUE" : "FALSE", _ => v?.ToString() ?? "NULL" };
    static string FormatSize(long b) { var s = new[] { "B", "KB", "MB", "GB" }; var i = 0; double d = b; while (d >= 1024 && i < 3) { d /= 1024; i++; } return $"{d:0.#} {s[i]}"; }
}

class BackupSettings
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? Database { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? BackupFolder { get; set; }
}

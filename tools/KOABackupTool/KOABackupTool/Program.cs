using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Npgsql;
using Spectre.Console;

namespace KOABackupTool;

class Program
{
    private static readonly string _executableDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
    private static string _host = "localhost";
    private static int _port = 5432;
    private static string _database = "DestekCRMServisBlazorDb";
    private static string _username = "postgres";
    private static string _password = "Fast123";
    private static string _backupFolder = GetDefaultBackupFolder();

    private static readonly string ConfigFile = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory, 
        "backup-settings.json");

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Kayıtlı ayarları yükle
        LoadSettings();

        if (IsAutoBackupMode(args))
        {
            ApplyCommandLineOverrides(args);
            await RunAutoBackupAsync();
            return;
        }

        AnsiConsole.Write(new FigletText("KOA Backup").Centered().Color(Color.Blue));
        AnsiConsole.MarkupLine("[grey]PostgreSQL Yedekleme Araci v1.2[/]");
        if (File.Exists(ConfigFile))
            AnsiConsole.MarkupLine($"[grey]Ayarlar yuklendi: {_host}:{_port}/{_database}[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[green]Ne yapmak istiyorsunuz?[/]").PageSize(12)
                .AddChoices(new[] { 
                    "1. Yedek Al (PostgreSQL)", 
                    "2. Yedek Yukle (PostgreSQL)", 
                    "3. PostgreSQL -> SQLite (Yedekten)", 
                    "4. PostgreSQL -> SQLite (Canli DB)",
                    "5. SQLite -> PostgreSQL",
                    "6. Yedek Sil",
                    "7. Ayarlar", 
                    "8. Klasor Ac", 
                    "9. Listele", 
                    "0. Cikis" 
                }));
            switch (choice[0])
            {
                case '1': await CreateBackupAsync(); break;
                case '2': await RestoreBackupAsync(); break;
                case '3': await ConvertToSqliteAsync(); break;
                case '4': await DirectPgToSqliteAsync(); break;
                case '5': await ConvertSqliteToPostgreSqlAsync(); break;
                case '6': await DeleteBackupsAsync(); break;
                case '7': ConfigureConnection(); break;
                case '8': OpenBackupFolder(); break;
                case '9': ListBackups(); break;
                case '0': return;
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
                    _backupFolder = ResolveConfiguredBackupFolder(settings.BackupFolder);
                }
            }
        }
        catch { /* İlk çalıştırma veya bozuk dosya */ }
    }

    static async Task RunAutoBackupAsync()
    {
        Directory.CreateDirectory(_backupFolder);

        AnsiConsole.Write(new Rule("[green]KOA Backup - Otomatik Tam Dump[/]").RuleStyle("grey"));
        AnsiConsole.MarkupLine($"[grey]Host:[/] {_host}");
        AnsiConsole.MarkupLine($"[grey]Port:[/] {_port}");
        AnsiConsole.MarkupLine($"[grey]Database:[/] {_database}");
        AnsiConsole.MarkupLine($"[grey]Yedek Klasoru:[/] {_backupFolder}");
        AnsiConsole.WriteLine();

        try
        {
            await CreateBackupAsync();
            Environment.ExitCode = 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Otomatik yedekleme hatasi: {ex.Message}[/]");
            Environment.ExitCode = 1;
        }
    }

    static void ApplyCommandLineOverrides(string[] args)
    {
        _host = GetArgumentValue(args, "--host") ?? _host;
        _database = GetArgumentValue(args, "--database") ?? _database;
        _username = GetArgumentValue(args, "--username") ?? _username;
        _password = GetArgumentValue(args, "--password") ?? _password;

        if (int.TryParse(GetArgumentValue(args, "--port"), out var port) && port > 0)
        {
            _port = port;
        }

        var outputFolder = GetArgumentValue(args, "--output");
        _backupFolder = string.IsNullOrWhiteSpace(outputFolder)
            ? GetDefaultBackupFolder()
            : Path.GetFullPath(outputFolder);
    }

    static bool IsAutoBackupMode(string[] args)
    {
        return args.Any(arg => arg.Equals("backup", StringComparison.OrdinalIgnoreCase)
            || arg.Equals("--backup", StringComparison.OrdinalIgnoreCase)
            || arg.Equals("--full-dump", StringComparison.OrdinalIgnoreCase)
            || arg.Equals("/backup", StringComparison.OrdinalIgnoreCase));
    }

    static string? GetArgumentValue(string[] args, string argumentName)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(argumentName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    static string GetDefaultBackupFolder()
    {
        var solutionRoot = FindSolutionRoot();
        if (!string.IsNullOrWhiteSpace(solutionRoot))
        {
            return Path.Combine(solutionRoot, "publish");
        }

        var normalizedBaseDirectory = Path.GetFullPath(_executableDirectory);
        if (string.Equals(Path.GetFileName(normalizedBaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)), "publish", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedBaseDirectory;
        }

        return Path.Combine(normalizedBaseDirectory, "publish");
    }

    static string ResolveConfiguredBackupFolder(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return GetDefaultBackupFolder();
        }

        var oldDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KOA_Backups");
        if (string.Equals(Path.GetFullPath(configuredPath), Path.GetFullPath(oldDefaultPath), StringComparison.OrdinalIgnoreCase))
        {
            return GetDefaultBackupFolder();
        }

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(configuredPath, _executableDirectory);
    }

    static string? FindSolutionRoot()
    {
        var current = new DirectoryInfo(_executableDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "KOAFiloServis.slnx")) ||
                Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
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

        // pg_dump yolunu bul
        var pgDumpPath = FindPgDump();

        if (pgDumpPath != null)
        {
            // pg_dump ile tam yedek al
            await CreateFullDumpAsync(pgDumpPath, backupFile, compressedFile);
        }
        else
        {
            // pg_dump bulunamazsa basit yedek al
            AnsiConsole.MarkupLine("[yellow]pg_dump bulunamadi, basit yedek alinacak...[/]");
            await CreateSimpleBackupAsync(backupFile, compressedFile);
        }
    }

    static string? FindPgDump()
    {
        // Olası pg_dump konumları
        var possiblePaths = new[]
        {
            @"C:\Program Files\PostgreSQL\17\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
            @"C:\Program Files (x86)\PostgreSQL\17\bin\pg_dump.exe",
            @"C:\Program Files (x86)\PostgreSQL\16\bin\pg_dump.exe",
            "pg_dump.exe", // PATH'te varsa
            "pg_dump"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path)) return path;
        }

        // PATH'te ara
        try
        {
            var result = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "where",
                Arguments = "pg_dump",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            result?.WaitForExit();
            var output = result?.StandardOutput.ReadToEnd()?.Trim();
            if (!string.IsNullOrEmpty(output) && File.Exists(output.Split('\n')[0]))
                return output.Split('\n')[0].Trim();
        }
        catch { }

        return null;
    }

    static async Task CreateFullDumpAsync(string pgDumpPath, string backupFile, string compressedFile)
    {
        try
        {
            await AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("pg_dump ile tam yedek aliniyor...", async ctx =>
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = $"-h {_host} -p {_port} -U {_username} -d {_database} -F p -b -v --no-owner --no-acl -f \"{backupFile}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.Environment["PGPASSWORD"] = _password;

                using var process = System.Diagnostics.Process.Start(psi);
                if (process == null) throw new Exception("pg_dump baslatilamadi");

                var errorTask = process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await errorTask;
                    throw new Exception($"pg_dump hatasi: {error}");
                }

                ctx.Status("Sikistiriliyor...");

                // Gzip sıkıştır
                await using (var fs = File.OpenRead(backupFile))
                await using (var cs = File.Create(compressedFile))
                await using (var gz = new GZipStream(cs, CompressionLevel.Optimal))
                    await fs.CopyToAsync(gz);

                File.Delete(backupFile);
            });

            var fileInfo = new FileInfo(compressedFile);
            AnsiConsole.MarkupLine($"[green]Tam yedek olusturuldu: {compressedFile}[/]");
            AnsiConsole.MarkupLine($"[grey]Boyut: {FormatSize(fileInfo.Length)}[/]");
        }
        catch (Exception ex) 
        { 
            AnsiConsole.MarkupLine($"[red]pg_dump hatasi: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[yellow]Basit yedek deneniyor...[/]");
            await CreateSimpleBackupAsync(backupFile, compressedFile);
        }
    }

    static async Task CreateSimpleBackupAsync(string backupFile, string compressedFile)
    {
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

        // Veritabanı yoksa oluştur
        await EnsureDatabaseExistsAsync();

        // psql yolunu bul ve kullan
        var psqlPath = FindPsql();
        if (psqlPath != null)
        {
            await RestoreWithPsqlAsync(psqlPath, file);
        }
        else
        {
            await RestoreWithNpgsqlAsync(file);
        }
    }

    static string? FindPsql()
    {
        var possiblePaths = new[]
        {
            @"C:\Program Files\PostgreSQL\17\bin\psql.exe",
            @"C:\Program Files\PostgreSQL\16\bin\psql.exe",
            @"C:\Program Files\PostgreSQL\15\bin\psql.exe",
            @"C:\Program Files\PostgreSQL\14\bin\psql.exe",
            "psql.exe",
            "psql"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    static async Task RestoreWithPsqlAsync(string psqlPath, string file)
    {
        try
        {
            await AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("psql ile geri yukleniyor...", async ctx =>
            {
                // Önce gz dosyasını aç
                string sqlFile = file;
                bool tempFile = false;

                if (file.EndsWith(".gz"))
                {
                    ctx.Status("Dosya aciliyor...");
                    sqlFile = Path.GetTempFileName();
                    tempFile = true;
                    await using var fs = File.OpenRead(file);
                    await using var gz = new GZipStream(fs, CompressionMode.Decompress);
                    await using var output = File.Create(sqlFile);
                    await gz.CopyToAsync(output);
                }

                ctx.Status("Veritabani hazirlaniyor...");

                // Önce mevcut tabloları sil
                var dropPsi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = psqlPath,
                    Arguments = $"-h {_host} -p {_port} -U {_username} -d {_database} -c \"DO $$ DECLARE r RECORD; BEGIN FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP EXECUTE 'DROP TABLE IF EXISTS \\\"' || r.tablename || '\\\" CASCADE'; END LOOP; END $$;\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                dropPsi.Environment["PGPASSWORD"] = _password;

                using (var dropProcess = System.Diagnostics.Process.Start(dropPsi))
                {
                    if (dropProcess != null) await dropProcess.WaitForExitAsync();
                }

                ctx.Status("SQL yukleniyor...");

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = psqlPath,
                    Arguments = $"-h {_host} -p {_port} -U {_username} -d {_database} -f \"{sqlFile}\" -v ON_ERROR_STOP=0",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.Environment["PGPASSWORD"] = _password;

                using var process = System.Diagnostics.Process.Start(psi);
                if (process == null) throw new Exception("psql baslatilamadi");

                await process.WaitForExitAsync();

                if (tempFile && File.Exists(sqlFile))
                    File.Delete(sqlFile);
            });

            AnsiConsole.MarkupLine("[green]Yedek basariyla yuklendi![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]psql hatasi: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[yellow]Alternatif yontem deneniyor...[/]");
            await RestoreWithNpgsqlAsync(file);
        }
    }

    static async Task RestoreWithNpgsqlAsync(string file)
    {
        try
        {
            string sql;
            if (file.EndsWith(".gz")) { await using var fs = File.OpenRead(file); await using var gz = new GZipStream(fs, CompressionMode.Decompress); using var sr = new StreamReader(gz); sql = await sr.ReadToEndAsync(); }
            else sql = await File.ReadAllTextAsync(file);

            using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            // Migration tablosunu oluştur
            await EnsureMigrationTableExistsAsync(conn);

            // Önce mevcut tabloları sil (foreign key sorunlarını önlemek için)
            AnsiConsole.MarkupLine("[grey]Mevcut tablolar siliniyor...[/]");
            using (var dropCmd = new NpgsqlCommand(@"
                DO $$ DECLARE r RECORD;
                BEGIN
                    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename != '__EFMigrationsHistory') LOOP
                        EXECUTE 'DROP TABLE IF EXISTS ""' || r.tablename || '"" CASCADE';
                    END LOOP;
                END $$;", conn))
            {
                await dropCmd.ExecuteNonQueryAsync();
            }

            AnsiConsole.MarkupLine("[grey]SQL komutlari calistiriliyor...[/]");

            // Foreign key kontrollerini devre dışı bırak
            using (var fkCmd = new NpgsqlCommand("SET session_replication_role = 'replica';", conn))
                await fkCmd.ExecuteNonQueryAsync();

            var statements = sql.Split(';').Where(s => !string.IsNullOrWhiteSpace(s) && !s.TrimStart().StartsWith("--")).ToList();
            int success = 0, failed = 0;
            var errors = new List<string>();

            foreach (var stmt in statements)
            {
                try 
                { 
                    using var cmd = new NpgsqlCommand(stmt.Trim(), conn); 
                    await cmd.ExecuteNonQueryAsync();
                    success++;
                } 
                catch (Exception ex) 
                { 
                    failed++;
                    if (errors.Count < 5) errors.Add(ex.Message.Split('\n')[0]);
                }
            }

            // Foreign key kontrollerini tekrar etkinleştir
            using (var fkCmd = new NpgsqlCommand("SET session_replication_role = 'origin';", conn))
                await fkCmd.ExecuteNonQueryAsync();

            AnsiConsole.MarkupLine($"[green]Yuklendi! ({success} basarili, {failed} atlandi)[/]");
            if (errors.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Bazi hatalar:[/]");
                foreach (var e in errors) AnsiConsole.MarkupLine($"[grey]  - {e}[/]");
            }
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

    #region PostgreSQL -> SQLite Dönüştürme

    static async Task ConvertToSqliteAsync()
    {
        if (!Directory.Exists(_backupFolder)) { AnsiConsole.MarkupLine("[red]Yedek klasoru yok[/]"); return; }

        var files = Directory.GetFiles(_backupFolder, "*.sql.gz")
            .Concat(Directory.GetFiles(_backupFolder, "*.sql"))
            .OrderByDescending(File.GetCreationTime).ToList();

        if (!files.Any()) { AnsiConsole.MarkupLine("[yellow]Yedek dosyasi yok[/]"); return; }

        var sel = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[green]Donusturulecek PostgreSQL yedegini secin:[/]")
            .AddChoices(files.Select(Path.GetFileName).Append("Geri")!));

        if (sel == "Geri") return;

        var sourceFile = files.First(f => Path.GetFileName(f) == sel);
        var sqliteFile = Path.Combine(_backupFolder, 
            Path.GetFileNameWithoutExtension(sourceFile).Replace(".sql", "") + "_sqlite.db");

        if (File.Exists(sqliteFile))
        {
            if (!AnsiConsole.Confirm($"[yellow]{Path.GetFileName(sqliteFile)} zaten var. Uzerine yazilsin mi?[/]", false))
                return;
            File.Delete(sqliteFile);
        }

        try
        {
            await AnsiConsole.Progress().StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]PostgreSQL -> SQLite donusturuluyor[/]", maxValue: 100);

                // 1. SQL dosyasını oku
                task.Description = "[grey]Yedek dosyasi okunuyor...[/]";
                string sql;
                if (sourceFile.EndsWith(".gz"))
                {
                    await using var fs = File.OpenRead(sourceFile);
                    await using var gz = new GZipStream(fs, CompressionMode.Decompress);
                    using var sr = new StreamReader(gz);
                    sql = await sr.ReadToEndAsync();
                }
                else
                {
                    sql = await File.ReadAllTextAsync(sourceFile);
                }
                task.Increment(10);

                // 2. SQLite veritabanı oluştur
                task.Description = "[grey]SQLite veritabani olusturuluyor...[/]";
                await using var conn = new SqliteConnection($"Data Source={sqliteFile}");
                await conn.OpenAsync();
                task.Increment(5);

                // 3. SQL'i ayrıştır ve tablolara böl
                task.Description = "[grey]SQL ifadeleri ayristiriliyor...[/]";
                var tableData = ParsePostgreSqlDump(sql);
                task.Increment(10);

                // 4. Migration tablosunu oluştur
                await CreateSqliteMigrationTableAsync(conn);
                task.Increment(5);

                // 5. Her tablo için dönüştürme yap
                var increment = 60.0 / Math.Max(tableData.Count, 1);
                int successTables = 0, failedTables = 0;
                var errors = new List<string>();

                foreach (var (tableName, tableInfo) in tableData)
                {
                    task.Description = $"[grey]Tablo: {tableName}[/]";
                    try
                    {
                        await ConvertTableToSqliteAsync(conn, tableName, tableInfo);
                        successTables++;
                    }
                    catch (Exception ex)
                    {
                        failedTables++;
                        if (errors.Count < 10) errors.Add($"{tableName}: {ex.Message.Split('\n')[0]}");
                    }
                    task.Increment(increment);
                }

                task.Increment(10);
                task.Description = "[green]Tamamlandi![/]";

                AnsiConsole.MarkupLine($"\n[green]SQLite veritabani olusturuldu: {sqliteFile}[/]");
                AnsiConsole.MarkupLine($"[grey]Tablolar: {successTables} basarili, {failedTables} hatali[/]");

                if (errors.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]Hatalar:[/]");
                    foreach (var e in errors) AnsiConsole.MarkupLine($"[grey]  - {e}[/]");
                }

                var fileInfo = new FileInfo(sqliteFile);
                AnsiConsole.MarkupLine($"[grey]Boyut: {FormatSize(fileInfo.Length)}[/]");
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Donusturme hatasi: {ex.Message}[/]");
        }
    }

    static Dictionary<string, TableInfo> ParsePostgreSqlDump(string sql)
    {
        var tables = new Dictionary<string, TableInfo>(StringComparer.OrdinalIgnoreCase);
        var lines = sql.Split('\n');

        string? currentTable = null;
        var createBuffer = new StringBuilder();
        bool inCreateTable = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            // CREATE TABLE tespit
            var createMatch = Regex.Match(line, @"CREATE TABLE\s+(?:IF NOT EXISTS\s+)?""?(\w+)""?\s*\(", RegexOptions.IgnoreCase);
            if (createMatch.Success)
            {
                currentTable = createMatch.Groups[1].Value;
                if (!tables.ContainsKey(currentTable))
                    tables[currentTable] = new TableInfo();
                inCreateTable = true;
                createBuffer.Clear();
                createBuffer.AppendLine(line);
                continue;
            }

            // CREATE TABLE devam
            if (inCreateTable && currentTable != null)
            {
                createBuffer.AppendLine(line);
                if (line.Contains(");") || (line.EndsWith(")") && !line.Contains("(")))
                {
                    tables[currentTable].CreateStatement = createBuffer.ToString();
                    inCreateTable = false;
                }
                continue;
            }

            // INSERT INTO tespit
            var insertMatch = Regex.Match(line, @"INSERT INTO\s+""?(\w+)""?\s*\(", RegexOptions.IgnoreCase);
            if (insertMatch.Success)
            {
                var tableName = insertMatch.Groups[1].Value;
                if (!tables.ContainsKey(tableName))
                    tables[tableName] = new TableInfo();
                tables[tableName].InsertStatements.Add(line);
            }
        }

        return tables;
    }

    static async Task CreateSqliteMigrationTableAsync(SqliteConnection conn)
    {
        var sql = @"CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
            ""MigrationId"" TEXT NOT NULL PRIMARY KEY,
            ""ProductVersion"" TEXT NOT NULL
        );";
        await using var cmd = new SqliteCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    static async Task ConvertTableToSqliteAsync(SqliteConnection conn, string tableName, TableInfo tableInfo)
    {
        // 1. CREATE TABLE'ı SQLite formatına çevir
        if (!string.IsNullOrEmpty(tableInfo.CreateStatement))
        {
            var sqliteCreate = ConvertCreateTableToSqlite(tableName, tableInfo.CreateStatement);
            try
            {
                await using var createCmd = new SqliteCommand(sqliteCreate, conn);
                await createCmd.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // Table exists
            {
                // Tablo zaten var, devam et
            }
        }

        // 2. INSERT'leri çevir ve çalıştır
        if (tableInfo.InsertStatements.Any())
        {
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                foreach (var insert in tableInfo.InsertStatements)
                {
                    var sqliteInsert = ConvertInsertToSqlite(insert);
                    try
                    {
                        await using var insertCmd = new SqliteCommand(sqliteInsert, conn, (SqliteTransaction)transaction);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                    catch { /* Tek satır hatası, devam et */ }
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    static string ConvertCreateTableToSqlite(string tableName, string pgCreate)
    {
        var result = pgCreate;

        // DROP TABLE ekle
        var dropTable = $"DROP TABLE IF EXISTS \"{tableName}\";\n";

        // PostgreSQL -> SQLite veri tipi dönüşümleri
        result = Regex.Replace(result, @"\bSERIAL\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bBIGSERIAL\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bSMALLSERIAL\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bcharacter varying\s*\(\d+\)", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bVARCHAR\s*\(\d+\)", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bCHAR\s*\(\d+\)", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bTEXT\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bTIMESTAMP\s*(WITHOUT TIME ZONE|WITH TIME ZONE)?\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bTIMESTAMPTZ\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDATE\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bTIME\s*(WITHOUT TIME ZONE|WITH TIME ZONE)?\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bBOOLEAN\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bBYTEA\b", "BLOB", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bUUID\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bJSON\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bJSONB\b", "TEXT", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bINTEGER\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bBIGINT\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bSMALLINT\b", "INTEGER", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDOUBLE PRECISION\b", "REAL", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bREAL\b", "REAL", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bNUMERIC\s*(\(\d+(,\s*\d+)?\))?", "REAL", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDECIMAL\s*(\(\d+(,\s*\d+)?\))?", "REAL", RegexOptions.IgnoreCase);

        // PostgreSQL spesifik ifadeleri kaldır
        result = Regex.Replace(result, @"\bDEFAULT\s+nextval\([^)]+\)", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDEFAULT\s+now\(\)", "DEFAULT CURRENT_TIMESTAMP", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDEFAULT\s+CURRENT_TIMESTAMP", "DEFAULT CURRENT_TIMESTAMP", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDEFAULT\s+'[^']*'::[\w\s]+", match => 
        {
            var val = Regex.Match(match.Value, @"'[^']*'").Value;
            return $"DEFAULT {val}";
        }, RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"::\w+(\[\])?", "", RegexOptions.IgnoreCase); // Type casts

        // CONSTRAINT isimlerini koru ama formatı düzelt
        result = Regex.Replace(result, @"CONSTRAINT\s+""(\w+)""\s+PRIMARY KEY", "PRIMARY KEY", RegexOptions.IgnoreCase);

        // ON CONFLICT, DEFERRABLE vb. kaldır
        result = Regex.Replace(result, @"\bON CONFLICT[^,)]*", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bDEFERRABLE[^,)]*", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bINITIALLY[^,)]*", "", RegexOptions.IgnoreCase);

        // Boş satırları ve fazla virgülleri temizle
        result = Regex.Replace(result, @",\s*,", ",");
        result = Regex.Replace(result, @",\s*\)", ")");

        return dropTable + result;
    }

    static string ConvertInsertToSqlite(string pgInsert)
    {
        var result = pgInsert;

        // TRUE/FALSE -> 1/0
        result = Regex.Replace(result, @"\bTRUE\b", "1", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\bFALSE\b", "0", RegexOptions.IgnoreCase);

        // Type casts kaldır
        result = Regex.Replace(result, @"::\w+(\[\])?", "", RegexOptions.IgnoreCase);

        // ON CONFLICT kaldır
        result = Regex.Replace(result, @"\bON CONFLICT[^;]*", "", RegexOptions.IgnoreCase);

        // E'...' escape syntax -> normal string
        result = Regex.Replace(result, @"E'([^']*)'", "'$1'");

        return result.TrimEnd(';') + ";";
    }

    #endregion

    #region Doğrudan PostgreSQL -> SQLite (Canlı DB)

    static async Task DirectPgToSqliteAsync()
    {
        if (string.IsNullOrEmpty(_password)) 
            _password = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Password:[/]").Secret('*'));

        Directory.CreateDirectory(_backupFolder);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var sqliteFile = Path.Combine(_backupFolder, $"{_database}_sqlite_{timestamp}.db");

        try
        {
            // Önce bağlantıyı test et
            using var testConn = new NpgsqlConnection(GetConnectionString());
            await testConn.OpenAsync();
            testConn.Close();

            await AnsiConsole.Progress().StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]PostgreSQL -> SQLite aktariliyor[/]", maxValue: 100);

                await using var pgConn = new NpgsqlConnection(GetConnectionString());
                await pgConn.OpenAsync();
                task.Increment(5);

                await using var sqliteConn = new SqliteConnection($"Data Source={sqliteFile}");
                await sqliteConn.OpenAsync();
                task.Increment(5);

                // Tabloları listele
                var tables = new List<(string name, long rowCount)>();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT t.tablename, 
                           COALESCE((SELECT reltuples::bigint FROM pg_class WHERE relname = t.tablename), 0) as rows
                    FROM pg_tables t 
                    WHERE schemaname = 'public' 
                    ORDER BY tablename", pgConn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        tables.Add((r.GetString(0), r.GetInt64(1)));
                }
                task.Increment(5);

                // Migration tablosunu oluştur
                await CreateSqliteMigrationTableAsync(sqliteConn);

                int successTables = 0, failedTables = 0;
                var errors = new List<string>();
                var increment = 80.0 / Math.Max(tables.Count, 1);

                foreach (var (tableName, rowCount) in tables)
                {
                    task.Description = $"[grey]Tablo: {tableName} ({rowCount} satir)[/]";
                    try
                    {
                        await TransferTableDirectAsync(pgConn, sqliteConn, tableName);
                        successTables++;
                    }
                    catch (Exception ex)
                    {
                        failedTables++;
                        if (errors.Count < 10) errors.Add($"{tableName}: {ex.Message.Split('\n')[0]}");
                    }
                    task.Increment(increment);
                }

                task.Increment(5);
                task.Description = "[green]Tamamlandi![/]";

                AnsiConsole.MarkupLine($"\n[green]SQLite veritabani olusturuldu: {sqliteFile}[/]");
                AnsiConsole.MarkupLine($"[grey]Tablolar: {successTables} basarili, {failedTables} hatali[/]");

                if (errors.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]Hatalar:[/]");
                    foreach (var e in errors) AnsiConsole.MarkupLine($"[grey]  - {e}[/]");
                }

                var fileInfo = new FileInfo(sqliteFile);
                AnsiConsole.MarkupLine($"[grey]Boyut: {FormatSize(fileInfo.Length)}[/]");
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Hata: {ex.Message}[/]");
        }
    }

    static async Task TransferTableDirectAsync(NpgsqlConnection pgConn, SqliteConnection sqliteConn, string tableName)
    {
        // 1. Tablo yapısını al
        var columns = new List<(string name, string pgType)>();
        using (var cmd = new NpgsqlCommand($@"
            SELECT a.attname, pg_catalog.format_type(a.atttypid, a.atttypmod)
            FROM pg_attribute a
            WHERE a.attrelid = '{tableName}'::regclass 
            AND a.attnum > 0 
            AND NOT a.attisdropped
            ORDER BY a.attnum", pgConn))
        using (var r = await cmd.ExecuteReaderAsync())
        {
            while (await r.ReadAsync())
                columns.Add((r.GetString(0), r.GetString(1)));
        }

        if (!columns.Any()) return;

        // 2. SQLite tablo oluştur
        var sqliteColumns = columns.Select(c => $"\"{c.name}\" {ConvertPgTypeToSqlite(c.pgType)}");
        var createSql = $"DROP TABLE IF EXISTS \"{tableName}\"; CREATE TABLE \"{tableName}\" ({string.Join(", ", sqliteColumns)});";

        await using (var createCmd = new SqliteCommand(createSql, sqliteConn))
            await createCmd.ExecuteNonQueryAsync();

        // 3. Verileri aktar
        using var selectCmd = new NpgsqlCommand($"SELECT * FROM \"{tableName}\"", pgConn);
        using var reader = await selectCmd.ExecuteReaderAsync();

        if (!reader.HasRows) return;

        await using var transaction = await sqliteConn.BeginTransactionAsync();
        try
        {
            var colNames = string.Join(", ", columns.Select(c => $"\"{c.name}\""));
            var paramNames = string.Join(", ", columns.Select((_, i) => $"@p{i}"));
            var insertSql = $"INSERT INTO \"{tableName}\" ({colNames}) VALUES ({paramNames});";

            while (await reader.ReadAsync())
            {
                await using var insertCmd = new SqliteCommand(insertSql, sqliteConn, (SqliteTransaction)transaction);
                for (int i = 0; i < columns.Count; i++)
                {
                    var value = reader.IsDBNull(i) ? DBNull.Value : ConvertPgValueToSqlite(reader.GetValue(i));
                    insertCmd.Parameters.AddWithValue($"@p{i}", value);
                }
                await insertCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    static string ConvertPgTypeToSqlite(string pgType)
    {
        var lower = pgType.ToLowerInvariant();
        if (lower.Contains("int") || lower.Contains("serial")) return "INTEGER";
        if (lower.Contains("bool")) return "INTEGER";
        if (lower.Contains("float") || lower.Contains("double") || lower.Contains("numeric") || lower.Contains("decimal") || lower.Contains("real")) return "REAL";
        if (lower.Contains("bytea")) return "BLOB";
        return "TEXT";
    }

    static object ConvertPgValueToSqlite(object value)
    {
        return value switch
        {
            bool b => b ? 1 : 0,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss.fffzzz"),
            TimeSpan ts => ts.ToString(),
            Guid g => g.ToString(),
            byte[] bytes => bytes,
            null => DBNull.Value,
            _ => value.ToString() ?? string.Empty
        };
    }

    #endregion

    #region SQLite -> PostgreSQL Dönüştürme

    static async Task ConvertSqliteToPostgreSqlAsync()
    {
        if (!Directory.Exists(_backupFolder)) { AnsiConsole.MarkupLine("[red]Yedek klasoru yok[/]"); return; }

        var files = Directory.GetFiles(_backupFolder, "*.db")
            .Concat(Directory.GetFiles(_backupFolder, "*.sqlite"))
            .Concat(Directory.GetFiles(_backupFolder, "*.sqlite3"))
            .OrderByDescending(File.GetCreationTime).ToList();

        if (!files.Any()) { AnsiConsole.MarkupLine("[yellow]SQLite dosyasi yok[/]"); return; }

        var sel = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[green]PostgreSQL'e aktarilacak SQLite veritabanini secin:[/]")
            .AddChoices(files.Select(Path.GetFileName).Append("Geri")!));

        if (sel == "Geri") return;

        var sourceFile = files.First(f => Path.GetFileName(f) == sel);

        AnsiConsole.MarkupLine($"[yellow]Hedef PostgreSQL: {_host}:{_port}/{_database}[/]");
        if (!AnsiConsole.Confirm("Devam edilsin mi? (Mevcut tablolar silinecek!)", false)) return;

        if (string.IsNullOrEmpty(_password))
            _password = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Password:[/]").Secret('*'));

        try
        {
            // Veritabanı yoksa oluştur
            await EnsureDatabaseExistsAsync();

            await AnsiConsole.Progress().StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]SQLite -> PostgreSQL aktariliyor[/]", maxValue: 100);

                await using var sqliteConn = new SqliteConnection($"Data Source={sourceFile};Mode=ReadOnly");
                await sqliteConn.OpenAsync();
                task.Increment(5);

                await using var pgConn = new NpgsqlConnection(GetConnectionString());
                await pgConn.OpenAsync();
                task.Increment(5);

                // SQLite tabloları listele
                var tables = new List<string>();
                using (var cmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name", sqliteConn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        tables.Add(r.GetString(0));
                }
                task.Increment(5);

                // Mevcut PostgreSQL tablolarını sil
                task.Description = "[grey]Mevcut tablolar siliniyor...[/]";
                using (var dropCmd = new NpgsqlCommand(@"
                    DO $$ DECLARE r RECORD;
                    BEGIN
                        FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                            EXECUTE 'DROP TABLE IF EXISTS ""' || r.tablename || '"" CASCADE';
                        END LOOP;
                    END $$;", pgConn))
                {
                    await dropCmd.ExecuteNonQueryAsync();
                }
                task.Increment(5);

                // Migration tablosunu oluştur
                await EnsureMigrationTableExistsAsync(pgConn);

                int successTables = 0, failedTables = 0;
                var errors = new List<string>();
                var increment = 75.0 / Math.Max(tables.Count, 1);

                foreach (var tableName in tables)
                {
                    task.Description = $"[grey]Tablo: {tableName}[/]";
                    try
                    {
                        await TransferSqliteTableToPostgreSqlAsync(sqliteConn, pgConn, tableName);
                        successTables++;
                    }
                    catch (Exception ex)
                    {
                        failedTables++;
                        if (errors.Count < 10) errors.Add($"{tableName}: {ex.Message.Split('\n')[0]}");
                    }
                    task.Increment(increment);
                }

                task.Increment(5);
                task.Description = "[green]Tamamlandi![/]";

                AnsiConsole.MarkupLine($"\n[green]PostgreSQL veritabanina aktarildi: {_database}[/]");
                AnsiConsole.MarkupLine($"[grey]Tablolar: {successTables} basarili, {failedTables} hatali[/]");

                if (errors.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]Hatalar:[/]");
                    foreach (var e in errors) AnsiConsole.MarkupLine($"[grey]  - {e}[/]");
                }
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Hata: {ex.Message}[/]");
        }
    }

    static async Task TransferSqliteTableToPostgreSqlAsync(SqliteConnection sqliteConn, NpgsqlConnection pgConn, string tableName)
    {
        // 1. SQLite tablo yapısını al
        var columns = new List<(string name, string type)>();
        using (var cmd = new SqliteCommand($"PRAGMA table_info(\"{tableName}\")", sqliteConn))
        using (var r = await cmd.ExecuteReaderAsync())
        {
            while (await r.ReadAsync())
            {
                var name = r.GetString(1);
                var type = r.GetString(2);
                columns.Add((name, type));
            }
        }

        if (!columns.Any()) return;

        // 2. PostgreSQL tablo oluştur
        var pgColumns = columns.Select(c => $"\"{c.name}\" {ConvertSqliteTypeToPg(c.type)}");

        // Primary key'i bul
        string? pkColumn = null;
        using (var pkCmd = new SqliteCommand($"PRAGMA table_info(\"{tableName}\")", sqliteConn))
        using (var pkReader = await pkCmd.ExecuteReaderAsync())
        {
            while (await pkReader.ReadAsync())
            {
                if (pkReader.GetInt32(5) == 1) // pk column
                {
                    pkColumn = pkReader.GetString(1);
                    break;
                }
            }
        }

        var createSql = $"CREATE TABLE \"{tableName}\" ({string.Join(", ", pgColumns)}";
        if (pkColumn != null)
            createSql += $", PRIMARY KEY (\"{pkColumn}\")";
        createSql += ");";

        await using (var createCmd = new NpgsqlCommand(createSql, pgConn))
            await createCmd.ExecuteNonQueryAsync();

        // 3. Verileri aktar
        using var selectCmd = new SqliteCommand($"SELECT * FROM \"{tableName}\"", sqliteConn);
        using var reader = await selectCmd.ExecuteReaderAsync();

        if (!reader.HasRows) return;

        var colNames = string.Join(", ", columns.Select(c => $"\"{c.name}\""));
        var paramNames = string.Join(", ", columns.Select((_, i) => $"@p{i}"));
        var insertSql = $"INSERT INTO \"{tableName}\" ({colNames}) VALUES ({paramNames});";

        while (await reader.ReadAsync())
        {
            await using var insertCmd = new NpgsqlCommand(insertSql, pgConn);
            for (int i = 0; i < columns.Count; i++)
            {
                var value = reader.IsDBNull(i) ? DBNull.Value : ConvertSqliteValueToPg(reader.GetValue(i), columns[i].type);
                insertCmd.Parameters.AddWithValue($"@p{i}", value ?? DBNull.Value);
            }
            await insertCmd.ExecuteNonQueryAsync();
        }
    }

    static string ConvertSqliteTypeToPg(string sqliteType)
    {
        var upper = sqliteType.ToUpperInvariant();
        if (upper.Contains("INT")) return "INTEGER";
        if (upper.Contains("REAL") || upper.Contains("FLOA") || upper.Contains("DOUB")) return "DOUBLE PRECISION";
        if (upper.Contains("BLOB")) return "BYTEA";
        if (upper.Contains("BOOL")) return "BOOLEAN";
        return "TEXT";
    }

    static object? ConvertSqliteValueToPg(object value, string sqliteType)
    {
        if (value == null || value == DBNull.Value) return DBNull.Value;

        var upper = sqliteType.ToUpperInvariant();

        // Boolean dönüşümü (SQLite'ta 0/1 olarak saklanır)
        if (upper.Contains("BOOL") && value is long l)
            return l != 0;

        return value;
    }

    #endregion

    #region Yedek Silme

    static async Task DeleteBackupsAsync()
    {
        if (!Directory.Exists(_backupFolder)) { AnsiConsole.MarkupLine("[red]Yedek klasoru yok[/]"); return; }

        var files = Directory.GetFiles(_backupFolder)
            .Where(f => f.EndsWith(".sql") || f.EndsWith(".sql.gz") || f.EndsWith(".db") || f.EndsWith(".sqlite") || f.EndsWith(".sqlite3"))
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList();

        if (!files.Any()) { AnsiConsole.MarkupLine("[yellow]Silinecek dosya yok[/]"); return; }

        var choices = files.Select(f => $"{f.Name} ({FormatSize(f.Length)}) - {f.CreationTime:dd.MM.yyyy HH:mm}").ToList();
        choices.Add("[red]Tum yedekleri sil[/]");
        choices.Add("Geri");

        var selected = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
            .Title("[green]Silinecek dosyalari secin (Space ile sec, Enter ile onayla):[/]")
            .PageSize(15)
            .AddChoices(choices));

        if (selected.Contains("Geri") || !selected.Any()) return;

        if (selected.Contains("[red]Tum yedekleri sil[/]"))
        {
            if (!AnsiConsole.Confirm("[red]TUM YEDEKLER SILINECEK! Emin misiniz?[/]", false)) return;

            foreach (var file in files)
            {
                try { file.Delete(); } catch { }
            }
            AnsiConsole.MarkupLine($"[green]{files.Count} dosya silindi.[/]");
            return;
        }

        int deleted = 0;
        foreach (var selection in selected)
        {
            var fileName = selection.Split(" (")[0];
            var file = files.FirstOrDefault(f => f.Name == fileName);
            if (file != null)
            {
                try { file.Delete(); deleted++; } 
                catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{file.Name} silinemedi: {ex.Message}[/]"); }
            }
        }

        AnsiConsole.MarkupLine($"[green]{deleted} dosya silindi.[/]");
        await Task.CompletedTask;
    }

    #endregion
}

class TableInfo
{
    public string? CreateStatement { get; set; }
    public List<string> InsertStatements { get; } = new();
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

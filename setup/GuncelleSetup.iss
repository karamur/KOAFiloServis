; ============================================================
; KOAFiloServis — Guncelleme Paketi (Hafif / Sadece Web Dosyalari)
; ============================================================
; Bu installer SADECE uygulama dosyalarini gunceller:
;   - Mevcut kurulum ZORUNLUDUR (yoksa hata verir)
;   - Guncelleme oncesi veritabani otomatik yedeklenir
;   - IIS sitesi durdurulur, dosyalar kopyalanir, yeniden baslatilir
;   - Konfigurasyonlar (dbsettings.json, appsettings.*.json) KORUNUR
;   - Veritabani ve kullanici dosyalari (uploads, logs) KORUNUR
;
; Ana paket (KOAFiloServisKurulum-*.exe) ile farki:
;   - IIS kurulum secenekleri yok (site zaten kurulu olmali)
;   - Lisans araci ve DataSync dahil DEGIL (opsiyonel)
;   - Cok daha kucuk dosya boyutu, daha hizli kurulum
; ============================================================

#define MyAppName    "KOAFiloServis"
#define MyAppExeName "KOAFiloServis.Web.exe"
#define MyInstallDir "C:\KOAFiloServis"
#define MyLisansExe  "KOAFiloServis.LisansDesktop.exe"
#define MyDataSyncExe "KOAFiloServis.DataSync.exe"

#ifndef MyAppVersion
#define MyAppVersion "1.0.5"
#endif

[Setup]
; Ana kurulumla ayni AppId — "guncelleme mi?" kontrolu icin
AppId={{8C5A9F12-4E2B-4B8A-9C2D-A7E6F1234567}
AppName={#MyAppName} Guncelleme
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher=KOA Yazilim
DefaultDirName={#MyInstallDir}
DisableDirPage=yes
; Guncelleme paketinde grup olusturma
DisableProgramGroupPage=yes
; Cikti
OutputBaseFilename=KOAFiloServisGuncelle-{#MyAppVersion}
#ifdef OutputDir
OutputDir={#OutputDir}
#else
OutputDir=output
#endif
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
ShowLanguageDialog=no
CloseApplications=force
RestartApplications=no
; Uninstall kaydini guncelleme paketi DEGISTIRMESIN (ana paket korur)
CreateUninstallRegKey=no
UpdateUninstallLogAppName=no
AllowNoIcons=yes
SetupLogging=yes

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Components]
Name: "web";      Description: "Web Uygulamasi (zorunlu)"; Flags: fixed; Types: full
Name: "lisans";   Description: "Lisans Yonetim Aracini da guncelle";   Types: full
Name: "datasync"; Description: "Veri Aktarim Aracini da guncelle";     Types: full

[Types]
Name: "full";    Description: "Tam Guncelleme (Web + Lisans + DataSync)"
Name: "webonly"; Description: "Sadece Web Uygulamasi"

[Files]
; Web — konfigurasyonlara DOKUNMA, sadece uygulama dosyalari
Source: "payload\Web\*"; \
    DestDir: "{app}"; \
    Excludes: "dbsettings.json,appsettings.Production.json,appsettings.json,portalsettings.json,backup_settings.json,*.db,*.db-shm,*.db-wal,logs\*,uploads\*,Backups\*"; \
    Flags: ignoreversion recursesubdirs createallsubdirs; \
    Components: web

; Lisans araci (opsiyonel)
Source: "payload\LisansDesktop\*"; \
    DestDir: "{app}\Lisans"; \
    Flags: ignoreversion recursesubdirs createallsubdirs; \
    Components: lisans

; DataSync (opsiyonel)
Source: "payload\DataSync\*"; \
    DestDir: "{app}\DataSync"; \
    Flags: ignoreversion recursesubdirs createallsubdirs; \
    Components: datasync

; Yardimci scriptler her zaman guncelle
Source: "scripts\backup-db.ps1";       DestDir: "{app}\scripts"; Flags: ignoreversion
Source: "scripts\iis-configure.ps1";   DestDir: "{app}\scripts"; Flags: ignoreversion
Source: "scripts\preinstall-check.ps1";DestDir: "{app}\scripts"; Flags: ignoreversion

[Code]
{ ============================================================
  Yardimci
  ============================================================ }

function GetInstallPath(): String;
var sPrevPath: String;
begin
  if RegQueryStringValue(HKLM,
        'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1',
        'InstallLocation', sPrevPath) then
    Result := sPrevPath
  else
    Result := '';
end;

function GetTimestamp(): String;
begin
  Result := GetDateTimeString('yyyymmdd-hhnnss', #0, #0);
end;

{ ============================================================
  Veritabani yedekleme
  ============================================================ }
procedure BackupDatabase(InstallPath: String);
var
  DbFile, BackupDir: String;
  ResultCode: Integer;
begin
  DbFile := InstallPath + '\KOAFiloServis';
  if not FileExists(DbFile) then Exit;

  BackupDir := InstallPath + '\Backups\db-' + GetTimestamp();

  Exec('cmd.exe', '/c mkdir "' + BackupDir + '"',
       '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec('cmd.exe', '/c copy /Y "' + DbFile + '" "' + BackupDir + '\KOAFiloServis"',
       '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if FileExists(InstallPath + '\KOAFiloServis-shm') then
    Exec('cmd.exe', '/c copy /Y "' + InstallPath + '\KOAFiloServis-shm" "' + BackupDir + '\KOAFiloServis-shm"',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if FileExists(InstallPath + '\KOAFiloServis-wal') then
    Exec('cmd.exe', '/c copy /Y "' + InstallPath + '\KOAFiloServis-wal" "' + BackupDir + '\KOAFiloServis-wal"',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Log('DB yedegi: ' + BackupDir);
end;

{ ============================================================
  IIS site durdur / baslat
  ============================================================ }
procedure StopIISSite();
var ResultCode: Integer;
begin
  Exec('cmd.exe',
       '/c "%windir%\system32\inetsrv\appcmd.exe" stop site /site.name:"KOAFiloServis"',
       '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure StartIISSite();
var ResultCode: Integer;
begin
  Exec('cmd.exe',
       '/c "%windir%\system32\inetsrv\appcmd.exe" start site /site.name:"KOAFiloServis"',
       '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

{ ============================================================
  Sihirbaz akisi
  ============================================================ }

{ Guncelleme paketi: mevcut kurulum OLMALIDIR }
function InitializeSetup(): Boolean;
var
  PrevPath, AppVer, Msg: String;
begin
  Result := True;
  PrevPath := GetInstallPath();

  if PrevPath = '' then
  begin
    MsgBox(
      '{#MyAppName} sistemde kurulu degil.' + #13#10#13#10 +
      'Bu paket GUNCELLEME icin tasarlanmistir.' + #13#10 +
      'Lutfen once ana kurulum paketini (KOAFiloServisKurulum-*.exe) calistirin.',
      mbError, MB_OK);
    Result := False;
    Exit;
  end;

  { Versiyonu kayittan oku }
  RegQueryStringValue(HKLM,
    'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1',
    'DisplayVersion', AppVer);

  Msg := 'Mevcut kurulum: ' + PrevPath;
  if AppVer <> '' then
    Msg := Msg + #13#10 + 'Kurulu versiyon : ' + AppVer;
  Msg := Msg + #13#10 + 'Yeni versiyon   : {#MyAppVersion}' + #13#10#13#10 +
         'Guncelleme yapilacak:' + #13#10 +
         '  * Veritabani otomatik yedeklenecek (Backups\db-<tarih>)' + #13#10 +
         '  * Konfigurasyonlar KORUNACAK' + #13#10 +
         '  * IIS sitesi yeniden baslatilacak' + #13#10#13#10 +
         'Devam etmek istiyor musunuz?';

  if MsgBox(Msg, mbConfirmation, MB_YESNO) = IDNO then
    Result := False;
end;

procedure InitializeWizard();
begin
  WizardForm.Caption := '{#MyAppName} {#MyAppVersion} — Guncelleme Sihirbazi';
end;

procedure CurStepChanged(CurStep: TSetupStep);
var PrevPath: String;
begin
  PrevPath := GetInstallPath();
  if PrevPath = '' then Exit;

  if CurStep = ssInstall then
  begin
    WizardForm.StatusLabel.Caption := 'Veritabani yedekleniyor...';
    BackupDatabase(PrevPath);
    WizardForm.StatusLabel.Caption := 'IIS sitesi durduruluyor...';
    StopIISSite();
  end;

  if CurStep = ssPostInstall then
  begin
    WizardForm.StatusLabel.Caption := 'IIS sitesi baslatiliyor...';
    StartIISSite();
  end;
end;

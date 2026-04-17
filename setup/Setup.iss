#define MyAppName "KOAFiloServis"
#define MyAppPublisher "KOA Yazilim"
#define MyAppURL "https://karamur.github.io/KOAFiloServis"
#define MyAppExeName "KOAFiloServis.Web.exe"
#define MyInstallDir "C:\KOAFiloServis"
#define MyDataDir "C:\KOAFiloServis\data"
#define MyLisansExe "KOAFiloServis.LisansDesktop.exe"
#define MyDataSyncExe "KOAFiloServis.DataSync.exe"

#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif

[Setup]
AppId={{8C5A9F12-4E2B-4B8A-9C2D-A7E6F1234567}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={#MyInstallDir}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
OutputBaseFilename=KOAFiloServisKurulum-{#MyAppVersion}
OutputDir=output
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} {#MyAppVersion}
ShowLanguageDialog=no
CloseApplications=force
RestartApplications=no
DisableProgramGroupPage=yes
AllowNoIcons=yes
SetupLogging=yes

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Types]
Name: "full"; Description: "Tam Kurulum"

[Components]
Name: "web"; Description: "KOAFiloServis Web (IIS)"; Types: full; Flags: fixed
Name: "lisans"; Description: "Lisans Yonetim Aracı"; Types: full
Name: "datasync"; Description: "Veri Aktarim Araci (PostgreSQL - SQLite)"; Types: full

[Tasks]
Name: "iisconfigure"; Description: "IIS Site ve AppPool'u otomatik yapılandır"; GroupDescription: "IIS:"; Flags: checkedonce
Name: "firewall"; Description: "Windows Guvenlik Duvarinda port aç (HTTP 5190)"; GroupDescription: "Firewall:"; Flags: checkedonce
Name: "browser"; Description: "Kurulum sonrası tarayicida aç"; GroupDescription: "Son adım:"; Flags: unchecked

[Files]
; Web uygulaması
Source: "payload\Web\*"; DestDir: "{app}"; Excludes: "dbsettings.json,appsettings.Production.json,*.db,logs\*,uploads\*"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: web

; İlk kurulumda dbsettings.json örneği (güncellemede UZANTISI dokunulmaz)
Source: "payload\Web\dbsettings.json"; DestDir: "{app}"; DestName: "dbsettings.json"; Flags: onlyifdoesntexist; Components: web

; Lisans aracı
Source: "payload\LisansDesktop\*"; DestDir: "{app}\Lisans"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: lisans

; DataSync
Source: "payload\DataSync\*"; DestDir: "{app}\DataSync"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: datasync

; IIS yapılandırma script'leri
Source: "scripts\iis-configure.ps1"; DestDir: "{app}\scripts"; Flags: ignoreversion
Source: "scripts\iis-remove.ps1"; DestDir: "{app}\scripts"; Flags: ignoreversion
Source: "scripts\preinstall-check.ps1"; DestDir: "{app}\scripts"; Flags: ignoreversion

[Dirs]
Name: "{app}\data"; Permissions: users-modify
Name: "{app}\uploads"; Permissions: users-modify
Name: "{app}\logs"; Permissions: users-modify
Name: "{app}\Backups"; Permissions: users-modify

[Icons]
Name: "{group}\{#MyAppName} Web'i Ac"; Filename: "http://localhost:5190"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\Lisans Yonetimi"; Filename: "{app}\Lisans\{#MyLisansExe}"; WorkingDir: "{app}\Lisans"; Components: lisans
Name: "{group}\Veri Aktarim (PG - SQLite)"; Filename: "{app}\DataSync\{#MyDataSyncExe}"; WorkingDir: "{app}\DataSync"; Components: datasync
Name: "{group}\Kurulum Klasorunu Ac"; Filename: "{app}"
Name: "{group}\Kaldır"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName} Web"; Filename: "http://localhost:5190"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: ; Flags: createonlyiffileexists

[Run]
; Ön kontrol (IIS + Hosting Bundle)
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\scripts\preinstall-check.ps1"""; \
    StatusMsg: "Gereksinimler kontrol ediliyor..."; \
    Flags: runhidden waituntilterminated

; IIS yapılandırma (seçildiyse)
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\scripts\iis-configure.ps1"" -InstallPath ""{app}"" -SiteName ""KOAFiloServis"" -Port 5190"; \
    StatusMsg: "IIS yapilandiriliyor..."; \
    Flags: runhidden waituntilterminated; \
    Tasks: iisconfigure

; Firewall kuralı
Filename: "netsh.exe"; \
    Parameters: "advfirewall firewall add rule name=""KOAFiloServis HTTP"" dir=in action=allow protocol=TCP localport=5190"; \
    StatusMsg: "Firewall kurali ekleniyor..."; \
    Flags: runhidden waituntilterminated; \
    Tasks: firewall

; Tarayıcıda aç
Filename: "http://localhost:5190"; \
    Flags: shellexec nowait postinstall; \
    Tasks: browser; \
    Description: "Uygulamayi tarayicida ac"

[UninstallRun]
; IIS site + app pool kaldırma
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\scripts\iis-remove.ps1"" -SiteName ""KOAFiloServis"""; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "RemoveIIS"

Filename: "netsh.exe"; \
    Parameters: "advfirewall firewall delete rule name=""KOAFiloServis HTTP"""; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "RemoveFirewall"

[UninstallDelete]
; Bu dosyalar kaldırma sonrası otomatik silinir (ama kullanıcı verisi SİLİNMEZ)
Type: filesandordirs; Name: "{app}\wwwroot\_framework"
Type: dirifempty; Name: "{app}\scripts"

[Code]
function IsUpgrade(): Boolean;
var
  sPrevPath: String;
begin
  Result := RegQueryStringValue(HKLM, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1', 'InstallLocation', sPrevPath) and (sPrevPath <> '');
end;

procedure InitializeWizard();
begin
  if IsUpgrade() then
    WizardForm.Caption := '{#MyAppName} Guncelleme Sihirbazi';
end;

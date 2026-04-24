; ============================================================
; KOAFiloServis — Lisans Yonetim Araci (Bagimsiz Installer)
; ============================================================
; Bu paket SADECE KOAFiloServisLisans.exe'yi kurar.
; Ana uygulama (IIS, Web) kurulmaz.
; Kullanim amaci: Yetkili bayi/yonetici pc'ye lisans uretme araci dagitmak.
; ============================================================

#define LisansAppName   "KOA Lisans Yonetimi"
#define LisansPublisher "KOA Yazilim"
#define LisansURL       "https://karamur.github.io/KOAFiloServis"
#define LisansExe       "KOAFiloServisLisans.exe"
#define LisansInstallDir "C:\KOALisans"

#ifndef LisansAppVersion
#define LisansAppVersion "1.0.0"
#endif

[Setup]
AppId={{2F7A1C33-8E4D-4A9B-BC5E-D3F8E9876543}
AppName={#LisansAppName}
AppVersion={#LisansAppVersion}
AppVerName={#LisansAppName} {#LisansAppVersion}
AppPublisher={#LisansPublisher}
AppPublisherURL={#LisansURL}
DefaultDirName={#LisansInstallDir}
DisableDirPage=no
DefaultGroupName={#LisansAppName}
OutputBaseFilename=KOALisansArac-{#LisansAppVersion}
#ifdef OutputDir
OutputDir={#OutputDir}
#else
OutputDir=output\v{#LisansAppVersion}
#endif
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#LisansExe}
UninstallDisplayName={#LisansAppName} {#LisansAppVersion}
ShowLanguageDialog=no
CloseApplications=force
DisableProgramGroupPage=yes
AllowNoIcons=yes
SetupLogging=yes

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Files]
Source: "payload\LisansDesktop\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
Name: "{app}\data"

[Icons]
Name: "{group}\{#LisansAppName}"; Filename: "{app}\{#LisansExe}"; WorkingDir: "{app}"
Name: "{group}\Kaldır"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#LisansAppName}"; Filename: "{app}\{#LisansExe}"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#LisansExe}"; \
    Description: "Lisans aracini hemen ac"; \
    Flags: postinstall nowait skipifsilent shellexec

[Code]
function IsUpgrade(): Boolean;
var sPrevPath: String;
begin
  Result := RegQueryStringValue(HKCU,
        'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1',
        'InstallLocation', sPrevPath) and (sPrevPath <> '');
end;

procedure InitializeWizard();
begin
  if IsUpgrade() then
    WizardForm.Caption := '{#LisansAppName} Guncelleme'
  else
    WizardForm.Caption := '{#LisansAppName} Kurulum';
end;

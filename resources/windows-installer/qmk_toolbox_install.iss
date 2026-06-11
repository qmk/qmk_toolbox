; QMK Toolbox — Inno Setup installer script
;
; Defines passed from make-win-installer.sh:
;   /DSourceDir=<path>  — directory containing qmk_toolbox.exe
;   /DOutputDir=<path>  — directory to write qmk_toolbox_install.exe into
;   /DIconFile=<path>   — path to output.ico

#define MyAppName      "QMK Toolbox"
#define MyAppVersion   "0.5.0"
#define MyAppPublisher "QMK"
#define MyAppURL       "https://qmk.fm"
#define MyAppExeName   "qmk_toolbox.exe"

[Setup]
AppId={{777330BE-14A3-42E5-BB86-8F9D30744097}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\{#MyAppName}
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=qmk_toolbox_install
SetupIconFile={#IconFile}
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#SourceDir}\qmk_toolbox.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


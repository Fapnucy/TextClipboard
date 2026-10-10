#define MyAppName "Text Clipboard"
#define MyAppVersion "1.3.0"
#define MyAppPublisher "Maciej Gosciniak"
#define MyAppExeName "TextClipboard.exe"

[Setup]
AppId={{EF92C48F-23AE-43CB-A4FC-734D1D597A65}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\TextClipboard
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=TextClipboard_Setup_1.3.0
SetupIconFile=..\TextClipboard\Assets\TextClipboard.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
CloseApplications=yes
RestartApplications=no

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional options:"; Flags: unchecked
Name: "startup"; Description: "Start Text Clipboard with Windows (in system tray)"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Text Clipboard"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall Text Clipboard"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Text Clipboard"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "TextClipboard"; ValueData: """{app}\{#MyAppExeName}"" --startup"; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Text Clipboard"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and (not IsTaskSelected('startup')) then
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'TextClipboard');
end;

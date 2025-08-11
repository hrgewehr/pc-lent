#define MyAppName "PCMedic"
#define MyAppVersion "0.1.0"
#define MyPublisher "Tactiva Consulting SRL"
#define MyAppExe "PCMedic.UI.exe"

[Setup]
AppId={{B3E29D49-54C2-4F5E-9C9B-0E3B0A1F0AAE}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyPublisher}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=PCMedic-Setup
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "..\dist\PCMedic\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Run]
Filename: "sc.exe"; Parameters: "create PCMedic.Agent binPath=\"\{app}\Agent\PCMedic.Agent.exe\" start= auto obj= \"LocalSystem\""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start PCMedic.Agent"; Flags: runhidden waituntilterminated

[Icons]
Name: "{group}\PCMedic"; Filename: "{app}\UI\{#MyAppExe}"

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop PCMedic.Agent"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "delete PCMedic.Agent"; Flags: runhidden waituntilterminated

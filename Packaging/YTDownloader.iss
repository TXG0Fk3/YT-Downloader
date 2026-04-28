#define MyAppName "YT Downloader"
#define MyAppPublisher "TXG0Fk3"
#define MyAppURL "https://github.com/TXG0Fk3/YTDownloader"
#define MyAppExeName "YTDownloader.exe"
#define MyProjectName "YTDownloader"
#define MyArch "x64"

#define AppPublishPath "..\YTDownloader\bin\Release\net10.0-windows10.0.26100.0\win-" + MyArch + "\publish\"
#define FullExePath AppPublishPath + MyAppExeName

#ifExist FullExePath
  #define MyAppVersion GetStringFileInfo(FullExePath, "ProductVersion")
  #if MyAppVersion == ""
    #define MyAppVersion GetFileVersion(FullExePath)
  #endif
#else
  #define MyAppVersion "0.0.0-BuildError"
#endif

[Setup]
AppId={{80672122-7039-460B-AD27-2CEF3682FBB6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

OutputDir=.
OutputBaseFilename={#MyProjectName}.Setup.{#MyArch}
SetupIconFile=..\YTDownloader.ico

Compression=lzma
SolidCompression=yes
WizardStyle=modern dynamic windows11

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#AppPublishPath}*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
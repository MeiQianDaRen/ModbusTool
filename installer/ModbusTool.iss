#define MyAppName "ModbusTool"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ModbusTool"
#define MyAppExeName "ModbusMaster.exe"

[Setup]
AppId={{7F4C4E6A-4D0C-4A7D-BF45-123456789001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={autopf}\ModbusTool
DefaultGroupName=ModbusTool

OutputDir=.
OutputBaseFilename=ModbusTool-Setup

Compression=lzma
SolidCompression=yes

ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Modbus Master"; Filename: "{app}\ModbusMaster.exe"
Name: "{group}\Modbus Slave"; Filename: "{app}\ModbusSlave.exe"

Name: "{autodesktop}\Modbus Master"; Filename: "{app}\ModbusMaster.exe"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

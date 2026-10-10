$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot

# Publish self-contained single-file .NET WPF exe (Windows x64)
dotnet publish '.\TextClipboard\TextClipboard.csproj' -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugSymbols=false -p:DebugType=None -o '.\publish'
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }
if (-not (Test-Path '.\publish\TextClipboard.exe')) { throw 'Missing published TextClipboard.exe' }

# Inno Setup 6 or 7; installed per-machine or per-user
$candidates = @(
    "$env:ProgramFiles\Inno Setup 7\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
)
$iscc = $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $iscc) {
    Write-Host 'Publication OK, but Inno Setup was not found.' -ForegroundColor Yellow
    Write-Host 'Install Inno Setup from https://jrsoftware.org/isdl.php and run this script again.'
    exit 1
}

& $iscc '.\Installer\TextClipboard.iss'
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compilation failed.' }
Write-Host 'Ready: Installer\Output\TextClipboard_Setup_1.1.1.exe' -ForegroundColor Green

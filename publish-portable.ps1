$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "TextClipboard\TextClipboard.csproj"
$outDir = Join-Path $PSScriptRoot "publish-portable"
dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugSymbols=false -p:DebugType=None -o $outDir
Write-Host "Pliki po publikacji: $outDir"
Write-Host "Aby przenosic rowniez dane, utworz plik portable.mode obok EXE."

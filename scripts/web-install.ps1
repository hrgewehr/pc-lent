$ErrorActionPreference="Stop"

$Owner = "ORG_OR_USER"   # <<< UPDATE (ex: org)
$Repo  = "PCMedic"       # <<< UPDATE

$AssetName = "PCMedic-win64.zip"
$api = "https://api.github.com/repos/$Owner/$Repo/releases/latest"
$rel = Invoke-RestMethod $api
$dl  = ($rel.assets | Where-Object name -eq $AssetName).browser_download_url
if (-not $dl) { throw "Nu găsesc asset $AssetName în ultimul release." }

$dst = "$env:TEMP\PCMedic.zip"
Invoke-WebRequest $dl -OutFile $dst
$target = "C:\Program Files\PCMedic"
if (Test-Path $target) { Remove-Item $target -Recurse -Force }
Expand-Archive $dst -DestinationPath $target -Force

& "$target\scripts\install-service.ps1"

# Shortcut
$Wsh = New-Object -ComObject WScript.Shell
$Lnk = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\PCMedic.lnk"
$Sc  = $Wsh.CreateShortcut($Lnk)
$Sc.TargetPath = "$target\UI\PCMedic.UI.exe"
$Sc.Save()

Write-Host "PCMedic instalat."

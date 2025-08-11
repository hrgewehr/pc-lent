param([int]$Port=7766)

# root repo (în script merge PSScriptRoot; dacă e gol, folosește folderul curent)
$repo = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($repo)) { $repo = (Get-Location).Path }

$agent = Join-Path $repo 'PCMedic.Agent'
$ui    = Join-Path $repo 'PCMedic.UI'

# oprește instanțele vechi ca să nu blocheze build-ul
Get-Process -Name 'PCMedic.Agent','PCMedic.UI' -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet build -c Release $repo | Out-Null

$env:ASPNETCORE_URLS = "http://localhost:$Port"
Start-Process powershell.exe -ArgumentList @('-NoExit','-Command',"cd `"$repo`"; dotnet run -c Release --project `"$agent`"")
Start-Sleep -Seconds 2
Start-Process powershell.exe -ArgumentList @('-NoExit','-Command',"cd `"$repo`"; dotnet run -c Release --project `"$ui`"")

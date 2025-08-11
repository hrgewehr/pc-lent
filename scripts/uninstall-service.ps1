$ErrorActionPreference = "Stop"
sc.exe stop "PCMedic.Agent" | Out-Null
sc.exe delete "PCMedic.Agent" | Out-Null
Write-Host "PCMedic Agent eliminat."

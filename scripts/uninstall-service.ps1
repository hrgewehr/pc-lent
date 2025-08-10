$ErrorActionPreference = "Stop"
sc.exe stop "PCMedicAgent" | Out-Null
sc.exe delete "PCMedicAgent" | Out-Null
Write-Host "PCMedic Agent eliminat."

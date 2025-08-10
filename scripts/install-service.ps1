param([string]$OutDir = "publish")
-dotnet publish .\PCMedic.Agent\PCMedic.Agent.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=false -o $OutDir
+$ErrorActionPreference = "Stop"
+dotnet publish .\PCMedic.Agent\PCMedic.Agent.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=false -o $OutDir
+$svcPath = (Resolve-Path "$OutDir\PCMedic.Agent.exe").Path
+if (-not (Get-Service -Name "PCMedicAgent" -ErrorAction SilentlyContinue)) {
+  sc.exe create "PCMedicAgent" binPath= "$svcPath" start= auto | Out-Null
+}
+sc.exe start "PCMedicAgent" | Out-Null
+Write-Host "PCMedic Agent instalat si pornit."

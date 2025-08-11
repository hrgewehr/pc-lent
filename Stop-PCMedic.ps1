Get-Process -Name "PCMedic.Agent","PCMedic.UI" -ErrorAction SilentlyContinue | Stop-Process -Force

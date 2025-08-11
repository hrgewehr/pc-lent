$svcName = "PCMedic.Agent"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$agent = Join-Path (Split-Path -Parent $here) "Agent\PCMedic.Agent.exe"

sc.exe create $svcName binPath= "`"$agent`"" start= auto obj= "LocalSystem"
sc.exe description $svcName "PCMedic monitoring agent"
sc.exe start $svcName

import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Separator } from "@/components/ui/separator";
import { toast } from "sonner";
import { 
  Activity, 
  HardDrive, 
  Cpu, 
  Thermometer, 
  AlertTriangle, 
  CheckCircle, 
  Settings,
  Download,
  Trash2,
  Play
} from "lucide-react";

interface Finding {
  id: string;
  severity: 'High' | 'Medium' | 'Low';
  message: string;
  action: string;
}

interface SystemSnapshot {
  timestamp: string;
  status: string;
  cpuUsagePercent: number;
  ramUsedGb: number;
  ramTotalGb: number;
  diskQueue: number;
  cpuTempC: number;
  gpuTempC: number;
  ssdFreePercent: number;
  hddFreePercent: number;
  wordCrashes24h: number;
  excelCrashes24h: number;
  findings: Finding[];
}

const Index = () => {
  const [snapshot, setSnapshot] = useState<SystemSnapshot | null>(null);
  const [isScanning, setIsScanning] = useState(false);
  const [lastScan, setLastScan] = useState<Date | null>(null);

  // Simulare date sistem pentru demo
  const generateMockSnapshot = (): SystemSnapshot => {
    const cpuUsage = Math.random() * 100;
    const ramUsed = 6 + Math.random() * 8;
    const ramTotal = 16;
    const diskQueue = Math.random() * 3;
    const cpuTemp = 35 + Math.random() * 50;
    const gpuTemp = 40 + Math.random() * 45;
    const ssdFree = 20 + Math.random() * 60;
    const hddFree = 30 + Math.random() * 50;
    const wordCrashes = Math.floor(Math.random() * 5);
    const excelCrashes = Math.floor(Math.random() * 3);

    const findings: Finding[] = [];
    
    if (ssdFree < 15) {
      findings.push({
        id: 'ssd.space.low',
        severity: 'Medium',
        message: `SSD liber ${ssdFree.toFixed(0)}% (<15%).`,
        action: 'cleanup'
      });
    }
    
    if (hddFree < 15) {
      findings.push({
        id: 'hdd.space.low', 
        severity: 'Medium',
        message: `HDD liber ${hddFree.toFixed(0)}% (<15%).`,
        action: 'cleanup'
      });
    }
    
    if (wordCrashes + excelCrashes >= 3) {
      findings.push({
        id: 'office.crash',
        severity: 'High', 
        message: `Crash-uri Word/Excel ≥ 3/24h.`,
        action: 'disable_office_addins_repair'
      });
    }
    
    if (cpuTemp >= 85 || gpuTemp >= 85) {
      findings.push({
        id: 'temp.high',
        severity: 'High',
        message: `Temperaturi ridicate (CPU ${cpuTemp.toFixed(0)}°C / GPU ${gpuTemp.toFixed(0)}°C).`,
        action: 'clean_fans_paste'
      });
    }
    
    if (diskQueue > 2) {
      findings.push({
        id: 'disk.queue',
        severity: 'Medium',
        message: `Disk queue ${diskQueue.toFixed(2)} (>2).`,
        action: 'check_hdd_defrag'
      });
    }

    return {
      timestamp: new Date().toISOString(),
      status: findings.length > 0 ? 'Issues Found' : 'Healthy',
      cpuUsagePercent: cpuUsage,
      ramUsedGb: ramUsed,
      ramTotalGb: ramTotal,
      diskQueue,
      cpuTempC: cpuTemp,
      gpuTempC: gpuTemp,
      ssdFreePercent: ssdFree,
      hddFreePercent: hddFree,
      wordCrashes24h: wordCrashes,
      excelCrashes24h: excelCrashes,
      findings
    };
  };

  const performScan = async () => {
    setIsScanning(true);
    toast.info("Scanning system...");
    
    // Simulare delay pentru scan
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    const newSnapshot = generateMockSnapshot();
    setSnapshot(newSnapshot);
    setLastScan(new Date());
    setIsScanning(false);
    
    if (newSnapshot.findings.length > 0) {
      toast.warning(`Found ${newSnapshot.findings.length} issue(s)`);
    } else {
      toast.success("System is healthy!");
    }
  };

  const runFix = async (action: string) => {
    toast.info(`Running ${action}...`);
    
    // Simulare delay pentru fix
    await new Promise(resolve => setTimeout(resolve, 3000));
    
    toast.success(`${action} completed successfully`);
    
    // Re-scan după fix
    setTimeout(performScan, 1000);
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High': return 'destructive';
      case 'Medium': return 'default';
      default: return 'secondary';
    }
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'High': return <AlertTriangle className="h-4 w-4 text-destructive" />;
      case 'Medium': return <AlertTriangle className="h-4 w-4 text-orange-500" />;
      default: return <AlertTriangle className="h-4 w-4 text-muted-foreground" />;
    }
  };

  useEffect(() => {
    // Scan inițial
    performScan();
  }, []);

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary/5 to-secondary/10 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Header */}
        <div className="text-center space-y-2">
          <h1 className="text-4xl font-bold tracking-tight">PCMedic</h1>
          <p className="text-muted-foreground">System Health Monitor & Repair Tool</p>
        </div>

        {/* Action Buttons */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Settings className="h-5 w-5" />
              Quick Actions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              <Button 
                onClick={performScan} 
                disabled={isScanning}
                className="flex items-center gap-2"
              >
                <Activity className="h-4 w-4" />
                {isScanning ? 'Scanning...' : 'Scan Now'}
              </Button>
              <Button 
                variant="outline" 
                onClick={() => runFix('SFC')}
                className="flex items-center gap-2"
              >
                <Play className="h-4 w-4" />
                Run SFC
              </Button>
              <Button 
                variant="outline" 
                onClick={() => runFix('DISM')}
                className="flex items-center gap-2"
              >
                <Play className="h-4 w-4" />
                Run DISM
              </Button>
              <Button 
                variant="outline" 
                onClick={() => runFix('Defrag HDD')}
                className="flex items-center gap-2"
              >
                <HardDrive className="h-4 w-4" />
                Defrag HDD
              </Button>
              <Button 
                variant="outline" 
                onClick={() => window.open('https://github.com/releases', '_blank')}
                className="flex items-center gap-2"
              >
                <Download className="h-4 w-4" />
                Check Updates
              </Button>
            </div>
          </CardContent>
        </Card>

        {snapshot && (
          <>
            {/* Status Overview */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium">System Status</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center gap-2">
                    {snapshot.status === 'Healthy' ? 
                      <CheckCircle className="h-5 w-5 text-green-500" /> : 
                      <AlertTriangle className="h-5 w-5 text-orange-500" />
                    }
                    <span className="font-semibold">{snapshot.status}</span>
                  </div>
                  {lastScan && (
                    <p className="text-xs text-muted-foreground mt-1">
                      Last scan: {lastScan.toLocaleTimeString()}
                    </p>
                  )}
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2">
                    <Cpu className="h-4 w-4" />
                    CPU Usage
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>{snapshot.cpuUsagePercent.toFixed(1)}%</span>
                      <span>{snapshot.cpuTempC.toFixed(0)}°C</span>
                    </div>
                    <Progress value={snapshot.cpuUsagePercent} />
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium">Memory</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>{snapshot.ramUsedGb.toFixed(1)} GB</span>
                      <span>{snapshot.ramTotalGb.toFixed(1)} GB</span>
                    </div>
                    <Progress value={(snapshot.ramUsedGb / snapshot.ramTotalGb) * 100} />
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium flex items-center gap-2">
                    <Thermometer className="h-4 w-4" />
                    Temperatures
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-1 text-sm">
                    <div className="flex justify-between">
                      <span>CPU:</span>
                      <span>{snapshot.cpuTempC.toFixed(0)}°C</span>
                    </div>
                    <div className="flex justify-between">
                      <span>GPU:</span>
                      <span>{snapshot.gpuTempC.toFixed(0)}°C</span>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>

            {/* Storage & System Health */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <HardDrive className="h-5 w-5" />
                    Storage Health
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <div className="flex justify-between text-sm mb-2">
                      <span>SSD Free Space</span>
                      <span>{snapshot.ssdFreePercent.toFixed(0)}%</span>
                    </div>
                    <Progress value={snapshot.ssdFreePercent} />
                  </div>
                  <div>
                    <div className="flex justify-between text-sm mb-2">
                      <span>HDD Free Space</span>
                      <span>{snapshot.hddFreePercent.toFixed(0)}%</span>
                    </div>
                    <Progress value={snapshot.hddFreePercent} />
                  </div>
                  <Separator />
                  <div className="text-sm space-y-1">
                    <div className="flex justify-between">
                      <span>Disk Queue:</span>
                      <span>{snapshot.diskQueue.toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Word Crashes (24h):</span>
                      <span>{snapshot.wordCrashes24h}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Excel Crashes (24h):</span>
                      <span>{snapshot.excelCrashes24h}</span>
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <AlertTriangle className="h-5 w-5" />
                    System Findings
                    <Badge variant="secondary">{snapshot.findings.length}</Badge>
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {snapshot.findings.length === 0 ? (
                    <div className="text-center py-8 text-muted-foreground">
                      <CheckCircle className="h-12 w-12 mx-auto mb-4 text-green-500" />
                      <p>No issues detected</p>
                      <p className="text-sm">Your system is running smoothly!</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {snapshot.findings.map((finding, index) => (
                        <Alert key={index}>
                          <div className="flex items-start gap-3">
                            {getSeverityIcon(finding.severity)}
                            <div className="flex-1 space-y-2">
                              <div className="flex items-center gap-2">
                                <Badge variant={getSeverityColor(finding.severity) as any}>
                                  {finding.severity}
                                </Badge>
                              </div>
                              <AlertDescription>{finding.message}</AlertDescription>
                              <Button 
                                size="sm" 
                                variant="outline"
                                onClick={() => runFix(finding.action)}
                                className="text-xs"
                              >
                                Fix: {finding.action.replace(/_/g, ' ')}
                              </Button>
                            </div>
                          </div>
                        </Alert>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default Index;

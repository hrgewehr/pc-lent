using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PCMedic.Shared.Models;

namespace PCMedic.Agent.Services {
  public class Worker : BackgroundService {
    private readonly ILogger<Worker> _log; private readonly SnapshotStore _store;
    public Worker(ILogger<Worker> log, SnapshotStore store) { _log = log; _store = store; }

    protected override async Task ExecuteAsync(CancellationToken ct) {
      using var hw = new HardwareCollector();
      while (!ct.IsCancellationRequested) {
        try {
          var disks = hw.GetDisks();
          var perf = hw.GetPerf();
          var (cpuT, _) = hw.GetTemps();
          var (ssdFree, hddFree) = hw.GetFreeSpace();
          var (wC, eC) = EventLogCollector.CountOfficeCrashesLast24h();
          var snap = new HealthSnapshot(DateTimeOffset.Now, "OK", disks, perf, wC, eC,
                                        double.IsNaN(cpuT) ? 0 : Math.Round(cpuT,1),
                                        Math.Round(ssdFree,1), Math.Round(hddFree,1),
                                        new());
          snap = snap with { Findings = RuleEngine.Evaluate(snap) };
          _store.Current = snap;
          OpLogger.Log($"Collected snapshot: findings={snap.Findings.Count}, CPU={snap.Perf.CpuUsagePercent:0}%, RAM={snap.Perf.RamUsedGb:0.0}/{snap.Perf.RamTotalGb:0.0}GB, DiskQ={snap.Perf.DiskQueue:0.00}");
        } catch (Exception ex) { _log.LogError(ex, "collect error"); OpLogger.Log($"Collect error: {ex.Message}"); }
        await Task.Delay(TimeSpan.FromMinutes(5), ct);
      }
    }
  }
}

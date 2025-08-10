using System;
using System.Collections.Generic;
using System.Linq;
using PCMedic.Shared.Models;

namespace PCMedic.Agent.Services {
  public static class RuleEngine {
    public static List<Finding> Evaluate(HealthSnapshot s) {
      var f = new List<Finding>();
      foreach (var d in s.Disks) {
        if (d.MediaType?.Contains("SSD", StringComparison.OrdinalIgnoreCase) == true) continue;
        if (d.PredictFailure == true || (d.ReallocatedSectors ?? 0) > 0)
          f.Add(new("disk.smart.bad", Severity.High, $"HDD {d.Model}: risc de eșec (SMART).", "backup_replace_hdd"));
      }
      if (s.SsdFreePercent < 15) f.Add(new("ssd.space.low", Severity.Medium, $"SSD liber {s.SsdFreePercent:0}% (<15%).", "cleanup"));
      if (s.HddFreePercent < 15) f.Add(new("hdd.space.low", Severity.Medium, $"HDD liber {s.HddFreePercent:0}% (<15%).", "cleanup"));
      if (s.WordCrashes24h + s.ExcelCrashes24h >= 3)
        f.Add(new("office.crash", Severity.High, "Crash-uri Word/Excel ≥ 3/24h.", "disable_office_addins_repair"));
      if (s.CpuTempC >= 85 || s.Perf.GpuTempC >= 85)
        f.Add(new("temp.high", Severity.High, $"Temperaturi ridicate (CPU {s.CpuTempC:0}°C / GPU {s.Perf.GpuTempC:0}°C).", "clean_fans_paste"));
      if (s.Perf.DiskQueue > 2) f.Add(new("disk.queue", Severity.Medium, $"Disk queue {s.Perf.DiskQueue:0.00} (>2).", "check_hdd_defrag"));
      return f;
    }
  }
}

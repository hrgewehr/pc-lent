using System;
using System.Collections.Generic;

namespace PCMedic.Shared.Models {
  public record HealthSnapshot(DateTimeOffset Timestamp, string Status,
    List<DiskSmart> Disks, PerfSnapshot Perf, int WordCrashes24h, int ExcelCrashes24h,
    double CpuTempC, double SsdFreePercent, double HddFreePercent, List<Finding> Findings);
}

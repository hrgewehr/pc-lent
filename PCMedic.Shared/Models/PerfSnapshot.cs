namespace PCMedic.Shared.Models {
  public record PerfSnapshot(double CpuUsagePercent, double RamUsedGb, double RamTotalGb, double DiskQueue, double GpuTempC);
}

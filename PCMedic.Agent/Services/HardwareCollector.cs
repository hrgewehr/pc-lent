using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Diagnostics;
using LibreHardwareMonitor.Hardware;
using PCMedic.Shared.Models;
using System.Collections.Generic;

namespace PCMedic.Agent.Services {
  public class HardwareCollector : IDisposable {
    private readonly Computer _pc = new() { IsCpuEnabled = true, IsGpuEnabled = true, IsMotherboardEnabled = true, IsStorageEnabled = true };
    private readonly PerformanceCounter _cpu = new("Processor", "% Processor Time", "_Total", true);
    private readonly PerformanceCounter _diskQ = new("PhysicalDisk", "Current Disk Queue Length", "_Total", true);

    public HardwareCollector() { _pc.Open(); _ = _cpu.NextValue(); _ = _diskQ.NextValue(); }

    public List<DiskSmart> GetDisks() {
      var list = new List<DiskSmart>();
      using var q1 = new ManagementObjectSearcher(@"root\\wmi", "SELECT * FROM MSStorageDriver_FailurePredictStatus");
      using var q2 = new ManagementObjectSearcher(@"root\\wmi", "SELECT * FROM MSStorageDriver_FailurePredictData");
      using var drives = new ManagementObjectSearcher("SELECT DeviceID,Model,MediaType FROM Win32_DiskDrive");

      var status = q1.Get().Cast<ManagementObject>().ToDictionary(
        o => (string)o["InstanceName"], o => (bool)o["PredictFailure"]);
      var raw = q2.Get().Cast<ManagementObject>().ToDictionary(
        o => (string)o["InstanceName"], o => (byte[])o["VendorSpecific"]);
      foreach (ManagementObject d in drives.Get()) {
        string model = (string)(d["Model"] ?? "Unknown");
        string media = (string)(d["MediaType"] ?? "Unknown");
        var match = status.Keys.FirstOrDefault(k => k.Contains(model, StringComparison.OrdinalIgnoreCase)) ?? status.Keys.FirstOrDefault();
        bool? predict = match != null ? status[match] : null;
        int? realloc = null;
        if (match != null && raw.TryGetValue(match, out var v)) {
          for (int i = 2; i + 12 < v.Length; i += 12)
            if (v[i] == 5) { realloc = v[i + 5] | (v[i + 6] << 8) | (v[i + 7] << 16) | (v[i + 8] << 24); break; }
        }
        list.Add(new DiskSmart((string)d["DeviceID"], model, predict, realloc, media));
      }
      return list;
    }

    public (double cpuTemp, double gpuTemp) GetTemps() {
      double cpu = double.NaN, gpu = double.NaN;
      _pc.Accept(new UpdateVisitor());
      foreach (var hw in _pc.Hardware) {
        hw.Update();
        if (hw.HardwareType == HardwareType.Cpu)
          cpu = hw.Sensors.Where(s => s.SensorType == SensorType.Temperature).Select(s => (double?)s.Value).Max() ?? cpu;
        if (hw.HardwareType is HardwareType.GpuAmd or HardwareType.GpuNvidia)
          gpu = hw.Sensors.Where(s => s.SensorType == SensorType.Temperature).Select(s => (double?)s.Value).Max() ?? gpu;
      }
      return (cpu, gpu);
    }

    public PerfSnapshot GetPerf() {
      double cpu = Math.Round(_cpu.NextValue(), 1);
      double dq = Math.Round(_diskQ.NextValue(), 2);
      var ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
      double ramTotal = ci.TotalPhysicalMemory / 1_073_741_824.0;
      double ramUsed = (ci.TotalPhysicalMemory - ci.AvailablePhysicalMemory) / 1_073_741_824.0;
      var (_, gpuT) = GetTemps();
      return new PerfSnapshot(cpu, ramUsed, ramTotal, dq, double.IsNaN(gpuT) ? 0 : Math.Round(gpuT,1));
    }

    public (double ssdFreePct, double hddFreePct) GetFreeSpace() {
      double ssd = 100, hdd = 100;
      foreach (var d in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady)) {
        double pct = 100.0 * d.TotalFreeSpace / d.TotalSize;
        var media = "Unknown";
        try {
          using var q = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{d.Name.TrimEnd('\\')}'}} WHERE AssocClass = Win32_LogicalDiskToPartition");
          var part = q.Get().Cast<ManagementObject>().FirstOrDefault();
          if (part != null) {
            using var q2 = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition");
            var dd = q2.Get().Cast<ManagementObject>().FirstOrDefault();
            media = (string)(dd?["MediaType"] ?? "Unknown");
          }
        } catch { }
        if ((media ?? "").Contains("SSD", StringComparison.OrdinalIgnoreCase)) ssd = Math.Min(ssd, pct);
        else hdd = Math.Min(hdd, pct);
      }
      return (double.IsInfinity(ssd) ? 100 : ssd, double.IsInfinity(hdd) ? 100 : hdd);
    }

    public void Dispose() { _pc.Close(); _cpu?.Dispose(); _diskQ?.Dispose(); }
  }
}

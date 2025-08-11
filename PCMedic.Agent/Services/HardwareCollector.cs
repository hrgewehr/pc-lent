using System;
using System.Collections.Generic;
using System.Diagnostics;               // PerformanceCounter
using System.IO;
using System.Linq;
using System.Management;                // WMI
using LibreHardwareMonitor.Hardware;    // HardwareType (CPU/GPU)
using PCMedic.Shared.Models;

namespace PCMedic.Agent.Services
{
    public class HardwareCollector : IDisposable
    {
        private readonly Computer _pc = new()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMotherboardEnabled = true,
            IsStorageEnabled = true
        };

        private readonly PerformanceCounter _cpu =
            new("Processor", "% Processor Time", "_Total", readOnly: true);

        private readonly PerformanceCounter _diskQ =
            new("PhysicalDisk", "Current Disk Queue Length", "_Total", readOnly: true);

        public HardwareCollector()
        {
            _pc.Open();
            _ = _cpu.NextValue();   // prime read
            _ = _diskQ.NextValue();
        }

        // ---------- DISK SMART ----------
        public List<DiskSmart> GetDisks()
        {
            var list = new List<DiskSmart>();

            using var qStatus = new ManagementObjectSearcher(@"root\wmi",
                "SELECT * FROM MSStorageDriver_FailurePredictStatus");
            using var qRaw = new ManagementObjectSearcher(@"root\wmi",
                "SELECT * FROM MSStorageDriver_FailurePredictData");
            using var qDrives = new ManagementObjectSearcher(
                "SELECT DeviceID,Model,MediaType,PNPDeviceID FROM Win32_DiskDrive");

            var status = qStatus.Get().Cast<ManagementObject>()
                .ToDictionary(o => (string)o["InstanceName"], o => (bool)o["PredictFailure"]);
            var raw = qRaw.Get().Cast<ManagementObject>()
                .ToDictionary(o => (string)o["InstanceName"], o => (byte[])o["VendorSpecific"]);

            foreach (ManagementObject d in qDrives.Get())
            {
                string devId = d["DeviceID"]?.ToString() ?? "Unknown";
                string model = d["Model"]?.ToString() ?? "Unknown";
                string media = d["MediaType"]?.ToString() ?? "Unknown";
                string pnp   = d["PNPDeviceID"]?.ToString() ?? "";

                string? key =
                    status.Keys.FirstOrDefault(k => k.Contains(model, StringComparison.OrdinalIgnoreCase)) ??
                    status.Keys.FirstOrDefault(k => k.Contains(pnp,   StringComparison.OrdinalIgnoreCase));

                bool? predict = key != null ? status[key] : null;
                int? realloc = null;

                if (key != null && raw.TryGetValue(key, out var blob))
                {
                    // atribut SMART 0x05 (Reallocated Sector Count)
                    for (int i = 2; i + 12 <= blob.Length; i += 12)
                    {
                        if (blob[i] == 5)
                        {
                            realloc = blob[i + 5]
                                   | (blob[i + 6] << 8)
                                   | (blob[i + 7] << 16)
                                   | (blob[i + 8] << 24);
                            break;
                        }
                    }
                }

                list.Add(new DiskSmart(devId, model, predict, realloc, media));
            }

            return list;
        }

        // ---------- TEMPERATURES ----------
        public (double cpu, double gpu) GetTemps()
        {
            double cpu = double.NaN, gpu = double.NaN;

            foreach (var hw in _pc.Hardware)
            {
                hw.Update();

                if (hw.HardwareType == HardwareType.Cpu)
                {
                    var t = hw.Sensors.Where(s => s.SensorType == SensorType.Temperature)
                                      .Select(s => (double?)s.Value).Max();
                    if (t.HasValue) cpu = t.Value;
                }

                if (hw.HardwareType is HardwareType.GpuAmd or HardwareType.GpuNvidia)
                {
                    var t = hw.Sensors.Where(s => s.SensorType == SensorType.Temperature)
                                      .Select(s => (double?)s.Value).Max();
                    if (t.HasValue) gpu = t.Value;
                }
            }

            return (cpu, gpu);
        }

        // ---------- PERF SNAPSHOT ----------
        public PerfSnapshot GetPerf()
        {
            double cpuPct = Math.Round(_cpu.NextValue(), 1);
            double dq = Math.Round(_diskQ.NextValue(), 2);

            // Memorie din WMI (KB)
            double ramTotalGb = 0, ramUsedGb = 0;
            using (var mos = new ManagementObjectSearcher(
                "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject o in mos.Get())
                {
                    double totalKb = Convert.ToDouble(o["TotalVisibleMemorySize"]);
                    double freeKb  = Convert.ToDouble(o["FreePhysicalMemory"]);
                    ramTotalGb = totalKb / (1024.0 * 1024.0);
                    ramUsedGb  = (totalKb - freeKb) / (1024.0 * 1024.0);
                    break;
                }
            }

            var (_, gpuT) = GetTemps();

            return new PerfSnapshot(cpuPct,
                                    Math.Round(ramUsedGb, 2),
                                    Math.Round(ramTotalGb, 2),
                                    dq,
                                    double.IsNaN(gpuT) ? 0 : Math.Round(gpuT, 1));
        }

        // ---------- FREE SPACE (min SSD/HDD) ----------
        public (double ssdFreePct, double hddFreePct) GetFreeSpace()
        {
            double ssd = double.PositiveInfinity;
            double hdd = double.PositiveInfinity;

            foreach (var ld in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady))
            {
                double pct = 100.0 * ld.TotalFreeSpace / ld.TotalSize;
                string media = GetMediaTypeForLogical(ld.Name);

                if (media.IndexOf("SSD", StringComparison.OrdinalIgnoreCase) >= 0)
                    ssd = Math.Min(ssd, pct);
                else
                    hdd = Math.Min(hdd, pct);
            }

            return (double.IsInfinity(ssd) ? 100 : Math.Round(ssd, 1),
                    double.IsInfinity(hdd) ? 100 : Math.Round(hdd, 1));
        }

        private static string GetMediaTypeForLogical(string logicalName)
        {
            try
            {
                string ld = logicalName.TrimEnd('\\');

                // LogicalDisk -> Partition
                string q1 = "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + ld +
                            "'} WHERE AssocClass=Win32_LogicalDiskToPartition";
                using var s1 = new ManagementObjectSearcher(q1);
                var part = s1.Get().Cast<ManagementObject>().FirstOrDefault();
                if (part != null)
                {
                    // Partition -> DiskDrive
                    string pid = part["DeviceID"]?.ToString() ?? "";
                    string q2 = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + pid +
                                "'} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                    using var s2 = new ManagementObjectSearcher(q2);
                    var dd = s2.Get().Cast<ManagementObject>().FirstOrDefault();
                    return dd?["MediaType"]?.ToString() ?? "Unknown";
                }
            }
            catch { }
            return "Unknown";
        }

        // ---------- AGGREGATE ----------
        public (List<DiskSmart> disks, PerfSnapshot perf, double cpuT, double ssdFree, double hddFree) Gather()
        {
            var disks = GetDisks();
            var perf  = GetPerf();
            var (cpuT, _) = GetTemps();
            var (ssd, hdd) = GetFreeSpace();
            return (disks, perf, double.IsNaN(cpuT) ? 0 : Math.Round(cpuT, 1), ssd, hdd);
        }

        public void Dispose()
        {
            try { _pc.Close(); } catch { }
            _cpu?.Dispose();
            _diskQ?.Dispose();
        }
    }
}

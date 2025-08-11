// using-urile trebuie să fie PRIMELE linii
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using PCMedic.Shared.Models; // pentru Finding

namespace PCMedic.UI
{
    public partial class MainWindow : Window
    {
        // API base
        private static readonly HttpClient Http =
            new HttpClient { BaseAddress = new Uri("http://localhost:7766") };

        private static readonly JsonSerializerOptions JsonOpts =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly List<Finding> _findings = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await RefreshAll();
        }

        // ===== XAML handlers vechi (compat) =====
        private async void Refresh_Click(object s, RoutedEventArgs e) => await RefreshAll();
        private async void Fix_Sfc(object s, RoutedEventArgs e)      => await RunFix("sfc");
        private async void Fix_Dism(object s, RoutedEventArgs e)     => await RunFix("dism");
        private async void Fix_Defrag(object s, RoutedEventArgs e)   => await RunFix("defrag-hdd");

        // ===== Handlere noi (dacă le legi în XAML) =====
        private async void Scan_Click(object s, RoutedEventArgs e)   => await RefreshAll();
        private async void RunSfc_Click(object s, RoutedEventArgs e) => await RunFix("sfc");
        private async void RunDism_Click(object s, RoutedEventArgs e)=> await RunFix("dism");
        private async void DefragHdd_Click(object s, RoutedEventArgs e)=> await RunFix("defrag-hdd");
        private async void CheckUpdates_Click(object s, RoutedEventArgs e) => await Updater.OpenLatestReleaseAsync();
        private void Uninstall_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("powershell.exe",
                  "-NoProfile -ExecutionPolicy Bypass -Command \"Start-Process sc.exe -ArgumentList 'stop PCMedic.Agent' -Verb runas -WindowStyle Hidden; Start-Sleep -s 2; Start-Process sc.exe -ArgumentList 'delete PCMedic.Agent' -Verb runas -WindowStyle Hidden\"")
                { UseShellExecute = false, CreateNoWindow = true };
                Process.Start(psi);
            }
            catch { }
        }

        // ===== UI helpers =====
        private ItemsControl? FindingsControl =>
            (ItemsControl?)FindName("FindingsGrid")
            ?? (ItemsControl?)FindName("lvFindings")
            ?? (ItemsControl?)FindName("dgFindings");

        private void BindFindings()
        {
            if (FindingsControl is ListView lv) lv.ItemsSource = _findings;
            else if (FindingsControl is DataGrid dg) dg.ItemsSource = _findings;
        }

        private void SetStatus(string text)
        {
            if (FindName("StatusText") is TextBlock tb) tb.Text = text;
            else Title = $"PCMedic — {text}";
        }

        // ===== Core =====
        private async Task RefreshAll()
        {
            try
            {
                var healthTask   = GetHealthNumbers();  // tolerant la schema
                var findingsTask = GetFindings();

                var (cpuPct, ramUsed, ramTotal, dq, cpuT, gpuT) = await healthTask;
                var findings = await findingsTask ?? new List<Finding>();

                _findings.Clear();
                _findings.AddRange(findings);
                BindFindings();

                SetStatus($"CPU {cpuPct:0}% | RAM {ramUsed:0.0}/{ramTotal:0.0} GB | DiskQ {dq:0.00} | CPU {cpuT:0}°C | GPU {gpuT:0}°C");
            }
            catch (Exception ex)
            {
                _findings.Clear();
                BindFindings();
                SetStatus($"API indisponibil. Pornește serviciul PCMedic.Agent. ({ex.Message})");
            }
        }

        private async Task RunFix(string action)
        {
            try
            {
                var resp = await Http.PostAsync($"/fix/{action}", content: null);
                resp.EnsureSuccessStatusCode();
                await RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Nu am putut rula '{action}': {ex.Message}",
                    "PCMedic", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task<List<Finding>?> GetFindings()
        {
            var resp = await Http.GetAsync("/findings");
            resp.EnsureSuccessStatusCode();
            using var s = await resp.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<List<Finding>>(s, JsonOpts);
        }

        // ===== Health tolerant la nume de câmpuri =====
        private async Task<(double cpuPct,double ramUsed,double ramTotal,double dq,double cpuT,double gpuT)>
            GetHealthNumbers()
        {
            var resp = await Http.GetAsync("/health/latest");
            resp.EnsureSuccessStatusCode();
            using var s = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            var root = doc.RootElement;

            var perf = GetObj(root, "perf");

            double cpuPct   = GetNum(perf, "cpuPct","cpu","cpu_percent");
            double ramUsed  = GetNum(perf, "ramUsedGb","ramUsed","ram_used_gb","ram_used");
            double ramTotal = GetNum(perf, "ramTotalGb","ramTotal","ram_total_gb","ram_total");
            double dq       = GetNum(perf, "diskQueue","diskQ","queue");

            double cpuT = GetNum(root, "cpuTempC","cpuT","cpu_temp");
            double gpuT = GetNum(root, "gpuTempC","gpuT","gpu_temp");

            return (cpuPct, ramUsed, ramTotal, dq, cpuT, gpuT);
        }

        // helpers JSON
        private static JsonElement GetObj(JsonElement e, params string[] names)
        {
            foreach (var n in names)
                if (e.ValueKind == JsonValueKind.Object && e.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.Object)
                    return v;
            return default;
        }

        private static double GetNum(JsonElement e, params string[] names)
        {
            if (e.ValueKind == JsonValueKind.Object)
            {
                foreach (var n in names)
                    if (e.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d))
                        return d;
            }
            else if (e.ValueKind == JsonValueKind.Number && e.TryGetDouble(out var d0)) return d0;

            return 0;
        }
    }
}

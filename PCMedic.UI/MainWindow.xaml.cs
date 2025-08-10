using System.Net.Http;
using System.Text.Json;
using System.Windows;
using PCMedic.Shared.Models;

namespace PCMedic.UI {
  public partial class MainWindow : Window {
    private readonly HttpClient _http = new() { BaseAddress = new System.Uri("http://localhost:5000") };
    public MainWindow() {
      InitializeComponent();
      _http.BaseAddress = new System.Uri("http://localhost:7766");
      _ = Refresh();
    }
    private async System.Threading.Tasks.Task Refresh() {
      try {
        var s = await _http.GetStringAsync("/health/latest");
        var snap = JsonSerializer.Deserialize<HealthSnapshot>(s, new JsonSerializerOptions{PropertyNameCaseInsensitive=true});
        StatusText.Text = $"CPU {snap.Perf.CpuUsagePercent:0}% | RAM {snap.Perf.RamUsedGb:0.0}/{snap.Perf.RamTotalGb:0.0} GB | DiskQ {snap.Perf.DiskQueue:0.00} | CPU {snap.CpuTempC:0}°C | GPU {snap.Perf.GpuTempC:0}°C";
        FindingsGrid.ItemsSource = snap.Findings;
      } catch { StatusText.Text = "API indisponibil. Pornește serviciul PCMedic.Agent."; }
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await Refresh();
    private async void Fix_Sfc(object sender, RoutedEventArgs e) => await _http.PostAsync("/fix/sfc", null);
    private async void Fix_Dism(object sender, RoutedEventArgs e) => await _http.PostAsync("/fix/dism", null);
    private async void Fix_Defrag(object sender, RoutedEventArgs e) => await _http.PostAsync("/fix/defrag-hdd", null);
  }
}

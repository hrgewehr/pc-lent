using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using PCMedic.Shared.Models;

namespace PCMedic.UI;

public partial class MainWindow : Window
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost:7766") };

    public MainWindow()
    {
        InitializeComponent();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var snapshot = await GetSnapshotAsync();
            if (snapshot != null)
            {
                TimestampText.Text = $"Timestamp: {snapshot.Timestamp:yyyy-MM-dd HH:mm:ss}";
                StatusText.Text = $"Status: {snapshot.Status}";
            }
            else
            {
                StatusText.Text = "Status: (no data)";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private async Task<HealthSnapshot?> GetSnapshotAsync()
    {
        var res = await _http.GetAsync("/health/latest");
        res.EnsureSuccessStatusCode();
        await using var s = await res.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<HealthSnapshot>(s, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }
}

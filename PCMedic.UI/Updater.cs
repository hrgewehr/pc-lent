using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PCMedic.UI
{
    public static class Updater
    {
        private const string Owner = "ORG_OR_USER"; // <<< UPDATE
        private const string Repo  = "PCMedic";     // <<< UPDATE

        public static async Task OpenLatestReleaseAsync()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("PCMedic-Updater");
            var rel = await http.GetFromJsonAsync<GitHubRelease>($"https://api.github.com/repos/{Owner}/{Repo}/releases/latest");
            var url = rel?.html_url ?? $"https://github.com/{Owner}/{Repo}/releases";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private record GitHubRelease(string html_url);
    }
}

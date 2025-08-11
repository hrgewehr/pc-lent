using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using PCMedic.Agent.Services;

var builder = WebApplication.CreateBuilder(args);

// Pornește pe 7766
builder.WebHost.UseUrls("http://localhost:7766");

// DI
builder.Services.AddSingleton<SnapshotStore>();
builder.Services.AddHostedService<Worker>();

// Activează rularea ca service DOAR când vei instala serviciul
builder.Host.UseWindowsService();

var app = builder.Build();
app.MapGet("/ping", () => "OK");

// Endpoints
var store = app.Services.GetRequiredService<SnapshotStore>();
app.MapGet("/health/latest", () => Results.Json(store.Current));
app.MapGet("/findings",      () => Results.Json(store.Current.Findings));
app.MapPost("/fix/{action}", async (string action) =>
    action.ToLowerInvariant() switch
    {
        "sfc"             => Results.Json(new { code = await RepairActions.Sfc() }),
        "dism"            => Results.Json(new { code = await RepairActions.Dism() }),
        "schedule-chkdsk" => Results.Json(new { code = await RepairActions.ScheduleChkdsk() }),
        "defrag-hdd"      => Results.Json(new { code = await RepairActions.Defrag("C:") }),
        _                 => Results.BadRequest(new { error = "unknown action" })
    });

app.Run();

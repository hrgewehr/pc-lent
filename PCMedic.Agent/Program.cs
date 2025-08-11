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
app.MapPost("/fix/{action}", async (string action) => {
    OpLogger.Log($"Fix requested: {action}");
    var lower = action.ToLowerInvariant();
    int code = lower switch
    {
        "sfc"             => await RepairActions.Sfc(),
        "dism"            => await RepairActions.Dism(),
        "schedule-chkdsk" => await RepairActions.ScheduleChkdsk(),
        "defrag-hdd"      => await RepairActions.Defrag("C:"),
        _                  => -1
    };
    if (code == -1) return Results.BadRequest(new { error = "unknown action" });
    OpLogger.Log($"Fix result: {action} => exitCode={code}");
    return Results.Json(new { code });
});

app.Run();

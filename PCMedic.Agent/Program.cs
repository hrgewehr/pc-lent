using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCMedic.Agent.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

builder.Services.AddSingleton<SnapshotStore>();
builder.Services.AddHostedService<PCMedic.Agent.Services.Worker>();

builder.WebHost.UseKestrel(options =>
{
    options.ListenLocalhost(7766);
});

var app = builder.Build();

var store = app.Services.GetRequiredService<SnapshotStore>();

app.MapGet("/", () => Results.Ok("PCMedic Agent running"));
app.MapGet("/health/latest", () => Results.Json(store.Current));
app.MapGet("/findings", () => Results.Json(store.Current.Findings));
app.MapPost("/fix/{action}", async (string action) => {
  switch (action.ToLowerInvariant()) {
    case "sfc": return Results.Json(new { code = await RepairActions.Sfc() });
    case "dism": return Results.Json(new { code = await RepairActions.Dism() });
    case "schedule-chkdsk": return Results.Json(new { code = await RepairActions.ScheduleChkdsk() });
    case "defrag-hdd": return Results.Json(new { code = await RepairActions.Defrag("C:") });
    default: return Results.BadRequest(new { error = "unknown action" });
  }
});

app.Run();

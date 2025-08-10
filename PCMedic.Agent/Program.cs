using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCMedic.Agent.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

builder.Services.AddSingleton<SnapshotStore>();
builder.Services.AddHostedService<PCMedic.Agent.Worker>();

builder.WebHost.UseKestrel(options =>
{
    options.ListenLocalhost(7766);
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok("PCMedic Agent running"));
app.MapGet("/health/latest", (SnapshotStore store) => Results.Json(store.Current));

app.Run();

using Ms1.Api.Data;
using Ms1.Api.Endpoints;
using Ms1.Api.Fehlerbehandlung;
using Ms1.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMs1Datenbank(builder.Configuration);
builder.Services.AddScoped<MitarbeiterService>();

// Fehler immer als ProblemDetails (RFC 9457) mit verständlicher Meldung
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
// Ungültiges JSON auch ausserhalb von Development als Exception melden, damit ApiExceptionHandler
// eine Meldung pro Feld zurückgeben kann (sonst nur ein leeres 400)
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();

await app.MigriereDatenbankFallsAktiviertAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health-Endpunkt fuer Smoke Tests und Monitoring (Ops)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ms1" }));

app.MapMitarbeiterEndpoints();

app.Run();

// Damit WebApplicationFactory<Program> in den Tests darauf zugreifen kann
public partial class Program;

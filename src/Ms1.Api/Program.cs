using System.Text.Json.Serialization;
using Ms1.Api.Data;
using Ms1.Api.Endpoints;
using Ms1.Api.Fehlerbehandlung;
using Ms1.Api.OpenApi;
using Ms1.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "MS1 Mitarbeiter API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Microservice 1 des Ticketsystems (M324, Team 3): Mitarbeiter erfassen (US1) und auslesen (US2). " +
            "Fehler kommen als ProblemDetails (RFC 9457).";
        // Keine Server-URL: Die Beschreibung gilt für jede Umgebung (lokal, Staging, Produktion)
        document.Servers = [];
        return Task.CompletedTask;
    });
    options.AddOperationTransformer<MitarbeiterBeispiele>();
});

// Zahlen nur als JSON-Zahl annehmen ("skilllevel": 4), nicht als Text ("skilllevel": "4").
// Der Standard für Web-APIs erlaubt beides; strikt passt zur Spezifikation und zur OpenAPI-Beschreibung.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
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

// OpenAPI-Beschreibung unter /openapi/v1.json, in allen Umgebungen, damit MS2 und das Team
// die Schnittstelle auch in Staging/Produktion nachschlagen können
app.MapOpenApi();

// Health-Endpunkt fuer Smoke Tests und Monitoring (Ops)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ms1" }))
    .WithTags("Betrieb")
    .WithSummary("Health-Check");

app.MapMitarbeiterEndpoints();

app.Run();

// Damit WebApplicationFactory<Program> in den Tests darauf zugreifen kann
public partial class Program;

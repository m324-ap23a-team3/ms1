using Ms1.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMs1Datenbank(builder.Configuration);

var app = builder.Build();

await app.MigriereDatenbankFallsAktiviertAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health-Endpunkt fuer Smoke Tests und Monitoring (Ops)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ms1" }));

app.Run();

// Damit WebApplicationFactory<Program> in den Tests darauf zugreifen kann
public partial class Program;

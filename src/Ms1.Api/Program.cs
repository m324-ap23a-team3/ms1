var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health-Endpunkt fuer Smoke Tests und Monitoring (Ops)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ms1" }));

app.Run();

// Damit WebApplicationFactory<Program> in den Tests darauf zugreifen kann
public partial class Program;

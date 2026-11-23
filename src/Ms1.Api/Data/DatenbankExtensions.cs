using Microsoft.EntityFrameworkCore;

namespace Ms1.Api.Data;

public static class DatenbankExtensions
{
    /// <summary>Name des Connection-Strings in der Konfiguration (Umgebungsvariable: <c>ConnectionStrings__Ms1Db</c>).</summary>
    public const string ConnectionStringName = "Ms1Db";

    /// <summary>
    /// MariaDB-Version, gegen die EF Core SQL erzeugt. Fest eingetragen statt <c>ServerVersion.AutoDetect</c>,
    /// damit die App (und die Tests) ohne laufende Datenbank starten.
    /// </summary>
    public static readonly MariaDbServerVersion ServerVersion = new(new Version(11, 4));

    /// <summary>
    /// Registriert den <see cref="Ms1DbContext"/>. Der Connection-String kommt nur aus der Konfiguration
    /// (appsettings, Umgebungsvariablen, Secrets) und steht nie im Code.
    /// </summary>
    public static IServiceCollection AddMs1Datenbank(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Ms1DbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection-String '{ConnectionStringName}' fehlt. Setze ihn in appsettings oder als Umgebungsvariable ConnectionStrings__{ConnectionStringName}.");

            // Wiederholt kurze Verbindungsfehler. Bewusst knapp (3 Versuche, max. 5 s), damit ein Client bei
            // einer ausgefallenen DB nach Sekunden eine 503-Meldung bekommt statt nach über einer Minute.
            options.UseMySql(connectionString, ServerVersion, mysql =>
                mysql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null));
        });

        services.AddScoped<IMitarbeiterRepository, MitarbeiterRepository>();

        return services;
    }

    /// <summary>
    /// Wendet beim Start ausstehende Migrationen an, wenn <c>Datenbank:MigrationBeimStart</c> auf <c>true</c> steht
    /// (lokal und im Container). In den Tests ist es aus.
    /// </summary>
    public static async Task MigriereDatenbankFallsAktiviertAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Datenbank:MigrationBeimStart"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Ms1DbContext>();
        await db.Database.MigrateAsync();
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ms1.Api.Data;
using Ms1.Api.Tests.Fakes;

namespace Ms1.Api.Tests;

/// <summary>
/// Startet die API im Speicher für die Tests. Statt der echten Datenbank ist
/// <see cref="FakeMitarbeiterRepository"/> registriert, und Migrationen beim Start sind aus.
/// So laufen die Tests bei allen lokal und in der Pipeline ohne MariaDB.
/// </summary>
public class Ms1ApiFactory : WebApplicationFactory<Program>
{
    /// <summary>Das Fake-Repository, das die API in den Tests verwendet.</summary>
    public FakeMitarbeiterRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Datenbank:MigrationBeimStart", "false");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMitarbeiterRepository>();
            services.AddSingleton<IMitarbeiterRepository>(Repository);
        });
    }
}

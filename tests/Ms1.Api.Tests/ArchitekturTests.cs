using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ms1.Api.Data;
using Ms1.Api.Tests.Fakes;

namespace Ms1.Api.Tests;

public class ArchitekturTests(Ms1ApiFactory factory) : IClassFixture<Ms1ApiFactory>
{
    /// <summary>
    /// In den Tests löst die API <see cref="IMitarbeiterRepository"/> zum Fake auf,
    /// nicht zur EF-Core-Implementierung. Damit braucht kein Test eine Datenbank.
    /// </summary>
    [Fact]
    public void Tests_verwenden_Fake_statt_Datenbank()
    {
        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IMitarbeiterRepository>();

        Assert.IsType<FakeMitarbeiterRepository>(repository);
    }

    /// <summary>
    /// Ausserhalb der Tests ist die EF-Core-Implementierung registriert.
    /// </summary>
    [Fact]
    public void Ohne_Testkonfiguration_ist_EF_Repository_registriert()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddMs1Datenbank(configuration);

        var registrierung = Assert.Single(services, s => s.ServiceType == typeof(IMitarbeiterRepository));
        Assert.Equal(typeof(MitarbeiterRepository), registrierung.ImplementationType);
    }
}

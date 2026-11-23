using Microsoft.EntityFrameworkCore;
using Ms1.Api.Data;
using Ms1.Api.Models;

namespace Ms1.Api.Tests.US2;

/// <summary>
/// Tests für <see cref="MitarbeiterRepository"/> mit dem EF-Core-InMemory-Provider (keine echte Datenbank).
/// Prüfen das Verhalten des Repositorys; SQL und MariaDB selbst werden erst im Integrationstest (P3) geprüft.
/// </summary>
public class MitarbeiterRepositoryTests
{
    private static Ms1DbContext NeuerContext(string datenbankName) => new(
        new DbContextOptionsBuilder<Ms1DbContext>().UseInMemoryDatabase(datenbankName).Options);

    /// <summary>
    /// AK4: <see cref="MitarbeiterRepository.AlleLesenAsync"/> liest ohne Change-Tracking.
    /// Nach dem Lesen verfolgt der DbContext keinen einzigen Eintrag, obwohl Mitarbeiter geladen wurden.
    /// </summary>
    [Fact]
    public async Task AlleLesen_verwendet_kein_Change_Tracking()
    {
        var name = Guid.NewGuid().ToString();
        await using (var schreiben = NeuerContext(name))
        {
            schreiben.Mitarbeiter.AddRange(
                new Mitarbeiter { Vorname = "Max", Nachname = "Muster", Beitrittsdatum = new DateOnly(2021, 5, 15), Skilllevel = 4 },
                new Mitarbeiter { Vorname = "Anna", Nachname = "Beispiel", Beitrittsdatum = new DateOnly(2020, 1, 31), Skilllevel = 2 });
            await schreiben.SaveChangesAsync();
        }

        await using var lesen = NeuerContext(name);
        var repository = new MitarbeiterRepository(lesen);

        var mitarbeiter = await repository.AlleLesenAsync();

        Assert.Equal(2, mitarbeiter.Count);
        Assert.Empty(lesen.ChangeTracker.Entries());
    }

    /// <summary>
    /// AK3: Die Liste ist nach ID sortiert, unabhängig von der Reihenfolge in der Datenbank.
    /// </summary>
    [Fact]
    public async Task AlleLesen_sortiert_nach_ID()
    {
        var name = Guid.NewGuid().ToString();
        await using (var schreiben = NeuerContext(name))
        {
            schreiben.Mitarbeiter.AddRange(
                new Mitarbeiter { Id = 3, Vorname = "C", Nachname = "C", Beitrittsdatum = new DateOnly(2020, 1, 1), Skilllevel = 1 },
                new Mitarbeiter { Id = 1, Vorname = "A", Nachname = "A", Beitrittsdatum = new DateOnly(2020, 1, 1), Skilllevel = 1 },
                new Mitarbeiter { Id = 2, Vorname = "B", Nachname = "B", Beitrittsdatum = new DateOnly(2020, 1, 1), Skilllevel = 1 });
            await schreiben.SaveChangesAsync();
        }

        await using var lesen = NeuerContext(name);

        var mitarbeiter = await new MitarbeiterRepository(lesen).AlleLesenAsync();

        Assert.Equal([1, 2, 3], mitarbeiter.Select(m => m.Id));
    }
}

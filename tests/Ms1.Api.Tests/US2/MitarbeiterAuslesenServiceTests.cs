using Microsoft.Extensions.Logging.Abstractions;
using Ms1.Api.Contracts;
using Ms1.Api.Models;
using Ms1.Api.Services;
using Ms1.Api.Tests.Fakes;

namespace Ms1.Api.Tests.US2;

/// <summary>
/// Unit Tests für US2 «Alle Mitarbeiter auslesen» auf Ebene <see cref="MitarbeiterService"/>.
/// Die Datenbank ist durch <see cref="FakeMitarbeiterRepository"/> ersetzt.
/// </summary>
public class MitarbeiterAuslesenServiceTests
{
    private readonly FakeMitarbeiterRepository _repository = new();
    private readonly MitarbeiterService _service;

    public MitarbeiterAuslesenServiceTests()
    {
        _service = new MitarbeiterService(_repository, NullLogger<MitarbeiterService>.Instance);
    }

    private async Task ErfasseAsync(string vorname, string nachname, DateOnly beitrittsdatum, int skilllevel) =>
        await _repository.HinzufuegenAsync(new Mitarbeiter
        {
            Vorname = vorname,
            Nachname = nachname,
            Beitrittsdatum = beitrittsdatum,
            Skilllevel = skilllevel,
        });

    /// <summary>
    /// Happy Path (AK1–3): Sind Mitarbeiter erfasst, liefert der Service alle in einer Liste,
    /// nach ID sortiert und mit allen Feldern (ID, Vorname, Nachname, Beitrittsdatum, Skilllevel).
    /// </summary>
    [Fact]
    public async Task Liste_mit_Eintraegen_enthaelt_alle_Mitarbeiter_mit_allen_Feldern()
    {
        await ErfasseAsync("Max", "Muster", new DateOnly(2021, 5, 15), 4);
        await ErfasseAsync("Anna", "Beispiel", new DateOnly(2020, 1, 31), 2);

        var ergebnis = await _service.AlleAuslesenAsync();

        Assert.Equal(ErgebnisArt.Erfolg, ergebnis.Art);
        var liste = ergebnis.Wert!;
        Assert.Equal(2, liste.Anzahl);
        Assert.Equal("2 Mitarbeiter gefunden.", liste.Meldung);
        Assert.Equal(
            [
                new MitarbeiterResponse(1, "Max", "Muster", "15.05.2021", 4),
                new MitarbeiterResponse(2, "Anna", "Beispiel", "31.01.2020", 2),
            ],
            liste.Mitarbeiter);
    }

    /// <summary>
    /// Sad Path / Randfall (AK5): Ist kein Mitarbeiter erfasst, ist das kein Fehler.
    /// Der Service liefert eine leere Liste und die Meldung «Es sind noch keine Mitarbeiter erfasst.».
    /// </summary>
    [Fact]
    public async Task Leere_Liste_liefert_Meldung()
    {
        var ergebnis = await _service.AlleAuslesenAsync();

        Assert.Equal(ErgebnisArt.Erfolg, ergebnis.Art);
        Assert.Equal(0, ergebnis.Wert!.Anzahl);
        Assert.Empty(ergebnis.Wert.Mitarbeiter);
        Assert.Equal("Es sind noch keine Mitarbeiter erfasst.", ergebnis.Wert.Meldung);
    }

    /// <summary>
    /// Sad Path (AK6): Wirft die Datenbank beim Laden einen Fehler, liefert der Service eine
    /// verständliche Fehlermeldung statt einer Exception, ohne technische Details.
    /// </summary>
    [Fact]
    public async Task Fehler_beim_Laden_liefert_klare_Fehlermeldung()
    {
        _repository.Fehler = new InvalidOperationException("Unable to connect to any of the specified MySQL hosts");

        var ergebnis = await _service.AlleAuslesenAsync();

        Assert.Equal(ErgebnisArt.Fehler, ergebnis.Art);
        Assert.Null(ergebnis.Wert);
        Assert.Equal(
            "Die Mitarbeiter konnten wegen eines Datenbankfehlers nicht geladen werden. Bitte später erneut versuchen.",
            ergebnis.Fehlermeldung);
    }

    /// <summary>
    /// Happy Path (AK4, viele Einträge): Auch 10'000 Mitarbeiter werden vollständig geliefert.
    /// </summary>
    [Fact]
    public async Task Viele_Mitarbeiter_werden_vollstaendig_geliefert()
    {
        for (var i = 1; i <= 10_000; i++)
        {
            await ErfasseAsync($"Vorname{i}", $"Nachname{i}", new DateOnly(2020, 1, 1), 1 + i % 5);
        }

        var ergebnis = await _service.AlleAuslesenAsync();

        Assert.Equal(10_000, ergebnis.Wert!.Anzahl);
        Assert.Equal(10_000, ergebnis.Wert.Mitarbeiter.Count);
    }
}

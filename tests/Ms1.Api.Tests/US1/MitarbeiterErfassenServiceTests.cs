using Microsoft.Extensions.Logging.Abstractions;
using Ms1.Api.Contracts;
using Ms1.Api.Services;
using Ms1.Api.Tests.Fakes;

namespace Ms1.Api.Tests.US1;

/// <summary>
/// Unit Tests für US1 «Mitarbeiter erfassen» auf Ebene <see cref="MitarbeiterService"/>.
/// Die Datenbank ist durch <see cref="FakeMitarbeiterRepository"/> ersetzt, die Tests brauchen also keine DB.
/// </summary>
public class MitarbeiterErfassenServiceTests
{
    private readonly FakeMitarbeiterRepository _repository = new();
    private readonly MitarbeiterService _service;

    public MitarbeiterErfassenServiceTests()
    {
        _service = new MitarbeiterService(_repository, NullLogger<MitarbeiterService>.Instance);
    }

    private static MitarbeiterErfassenRequest GueltigerRequest() => new("Max", "Muster", "15.05.2021", 4);

    // ---------- Happy Path ----------

    /// <summary>
    /// Happy Path (AK1–5): Ein vollständiger, gültiger Mitarbeiter wird gespeichert.
    /// Erwartet: Ergebnis «Erfolg», genau ein Eintrag im Repository mit allen Feldern
    /// und eine automatisch vergebene ID.
    /// </summary>
    [Fact]
    public async Task Gueltiger_Mitarbeiter_wird_gespeichert()
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest());

        Assert.Equal(ErgebnisArt.Erfolg, ergebnis.Art);
        var gespeichert = Assert.Single(_repository.Gespeichert);
        Assert.Equal("Max", gespeichert.Vorname);
        Assert.Equal("Muster", gespeichert.Nachname);
        Assert.Equal(new DateOnly(2021, 5, 15), gespeichert.Beitrittsdatum);
        Assert.Equal(4, gespeichert.Skilllevel);

        Assert.NotNull(ergebnis.Wert);
        Assert.Equal(1, ergebnis.Wert.Id);
        Assert.Equal("15.05.2021", ergebnis.Wert.Beitrittsdatum);
    }

    /// <summary>
    /// Happy Path (AK6, Grenzwerte): Die Grenzwerte 1 und 5 sind gültige Skilllevel.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Skilllevel_an_den_Grenzen_1_und_5_ist_gueltig(int skilllevel)
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Skilllevel = skilllevel });

        Assert.Equal(ErgebnisArt.Erfolg, ergebnis.Art);
        Assert.Equal(skilllevel, Assert.Single(_repository.Gespeichert).Skilllevel);
    }

    /// <summary>
    /// Happy Path (AK3): Das Beitrittsdatum wird im Format der Spezifikation (TT.MM.JJJJ)
    /// und im ISO-Format (JJJJ-MM-TT) angenommen und ergibt dasselbe Datum.
    /// </summary>
    [Theory]
    [InlineData("15.05.2021")]
    [InlineData("2021-05-15")]
    [InlineData("15.5.2021")]
    public async Task Beitrittsdatum_wird_in_beiden_Formaten_angenommen(string eingabe)
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Beitrittsdatum = eingabe });

        Assert.Equal(ErgebnisArt.Erfolg, ergebnis.Art);
        Assert.Equal(new DateOnly(2021, 5, 15), Assert.Single(_repository.Gespeichert).Beitrittsdatum);
    }

    /// <summary>
    /// Happy Path (AK5): Zwei erfasste Mitarbeiter erhalten unterschiedliche IDs.
    /// </summary>
    [Fact]
    public async Task Jeder_Mitarbeiter_erhaelt_eine_eigene_ID()
    {
        var erster = await _service.ErfassenAsync(GueltigerRequest());
        var zweiter = await _service.ErfassenAsync(GueltigerRequest() with { Vorname = "Anna" });

        Assert.NotEqual(erster.Wert!.Id, zweiter.Wert!.Id);
    }

    /// <summary>
    /// Happy Path: Leerzeichen am Anfang und Ende der Namen werden entfernt, bevor gespeichert wird.
    /// </summary>
    [Fact]
    public async Task Namen_werden_getrimmt()
    {
        await _service.ErfassenAsync(GueltigerRequest() with { Vorname = "  Max ", Nachname = " Muster  " });

        var gespeichert = Assert.Single(_repository.Gespeichert);
        Assert.Equal("Max", gespeichert.Vorname);
        Assert.Equal("Muster", gespeichert.Nachname);
    }

    // ---------- Sad Path ----------

    /// <summary>
    /// Sad Path (AK6): Skilllevel ausserhalb von 1–5 (0 und 6) wird abgelehnt.
    /// Erwartet: Ergebnis «Ungueltig», Meldung beim Feld <c>skilllevel</c>, nichts gespeichert.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Skilllevel_0_und_6_werden_abgelehnt(int skilllevel)
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Skilllevel = skilllevel });

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        var meldung = Assert.Single(ergebnis.Validierungsfehler["skilllevel"]);
        Assert.Contains("zwischen 1 und 5", meldung);
        Assert.Empty(_repository.Gespeichert);
    }

    /// <summary>
    /// Sad Path (AK1, AK8): Ein fehlender, leerer oder nur aus Leerzeichen bestehender Vorname wird abgelehnt.
    /// Erwartet: Ergebnis «Ungueltig», verständliche Meldung beim Feld <c>vorname</c>, nichts gespeichert.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Fehlender_Vorname_wird_abgelehnt(string? vorname)
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Vorname = vorname });

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Equal(["Vorname ist ein Pflichtfeld."], ergebnis.Validierungsfehler["vorname"]);
        Assert.Single(ergebnis.Validierungsfehler);
        Assert.Empty(_repository.Gespeichert);
    }

    /// <summary>
    /// Sad Path (AK2): Ein fehlender Nachname wird abgelehnt.
    /// </summary>
    [Fact]
    public async Task Fehlender_Nachname_wird_abgelehnt()
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Nachname = null });

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Equal(["Nachname ist ein Pflichtfeld."], ergebnis.Validierungsfehler["nachname"]);
    }

    /// <summary>
    /// Sad Path: Ein Name mit mehr als 100 Zeichen (Spaltenlänge in der DB) wird abgelehnt.
    /// </summary>
    [Fact]
    public async Task Zu_langer_Vorname_wird_abgelehnt()
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Vorname = new string('a', 101) });

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Contains("höchstens 100 Zeichen", Assert.Single(ergebnis.Validierungsfehler["vorname"]));
    }

    /// <summary>
    /// Sad Path (AK3, AK8): Fehlendes oder ungültiges Beitrittsdatum (falsches Format, 31. Februar, Text)
    /// wird mit einer Meldung abgelehnt, die das erwartete Format nennt.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("31.02.2021")]
    [InlineData("05/15/2021")]
    [InlineData("gestern")]
    public async Task Ungueltiges_Beitrittsdatum_wird_abgelehnt(string? beitrittsdatum)
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Beitrittsdatum = beitrittsdatum });

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Contains("15.05.2021", Assert.Single(ergebnis.Validierungsfehler["beitrittsdatum"]));
        Assert.Empty(_repository.Gespeichert);
    }

    /// <summary>
    /// Sad Path (AK4): Ein fehlender Skilllevel wird abgelehnt.
    /// </summary>
    [Fact]
    public async Task Fehlender_Skilllevel_wird_abgelehnt()
    {
        var ergebnis = await _service.ErfassenAsync(GueltigerRequest() with { Skilllevel = null });

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Contains("Pflichtfeld", Assert.Single(ergebnis.Validierungsfehler["skilllevel"]));
    }

    /// <summary>
    /// Sad Path (AK8): Sind mehrere Felder falsch, gibt es für jedes Feld eine eigene Meldung,
    /// damit der Administrator alles auf einmal korrigieren kann.
    /// </summary>
    [Fact]
    public async Task Mehrere_Fehler_ergeben_eine_Meldung_pro_Feld()
    {
        var ergebnis = await _service.ErfassenAsync(new MitarbeiterErfassenRequest(null, null, "kein Datum", 9));

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Equal(
            ["beitrittsdatum", "nachname", "skilllevel", "vorname"],
            ergebnis.Validierungsfehler.Keys.Order());
    }

    /// <summary>
    /// Sad Path: Ohne Request-Body gibt es eine Meldung, welche Felder erwartet werden.
    /// </summary>
    [Fact]
    public async Task Fehlender_Request_wird_abgelehnt()
    {
        var ergebnis = await _service.ErfassenAsync(null);

        Assert.Equal(ErgebnisArt.Ungueltig, ergebnis.Art);
        Assert.Contains("skilllevel", Assert.Single(ergebnis.Validierungsfehler["body"]));
    }

    /// <summary>
    /// Sad Path: Ist die Datenbank nicht erreichbar, liefert der Service eine verständliche
    /// Fehlermeldung statt einer Exception.
    /// </summary>
    [Fact]
    public async Task Datenbankfehler_beim_Speichern_ergibt_Fehlermeldung()
    {
        _repository.Fehler = new InvalidOperationException("Verbindung zur Datenbank fehlgeschlagen");

        var ergebnis = await _service.ErfassenAsync(GueltigerRequest());

        Assert.Equal(ErgebnisArt.Fehler, ergebnis.Art);
        Assert.Contains("nicht gespeichert", ergebnis.Fehlermeldung);
    }
}

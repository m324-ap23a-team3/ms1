using System.Globalization;
using Ms1.Api.Models;

namespace Ms1.Api.Contracts;

/// <summary>
/// Ein Mitarbeiter, wie ihn die API zurückgibt (US1 und US2).
/// </summary>
/// <param name="Id">Eindeutige, automatisch vergebene ID. MS2 referenziert damit den Mitarbeiter.</param>
/// <param name="Vorname">Vorname.</param>
/// <param name="Nachname">Nachname.</param>
/// <param name="Beitrittsdatum">Firmeneintritt im Format TT.MM.JJJJ, z. B. «15.05.2021».</param>
/// <param name="Skilllevel">Skilllevel 1 bis 5.</param>
public record MitarbeiterResponse(
    int Id,
    string Vorname,
    string Nachname,
    string Beitrittsdatum,
    int Skilllevel)
{
    /// <summary>Format, in dem das Beitrittsdatum ausgegeben wird (wie im Beispiel der Spezifikation).</summary>
    public const string DatumsFormat = "dd.MM.yyyy";

    public static MitarbeiterResponse Aus(Mitarbeiter mitarbeiter) => new(
        mitarbeiter.Id,
        mitarbeiter.Vorname,
        mitarbeiter.Nachname,
        mitarbeiter.Beitrittsdatum.ToString(DatumsFormat, CultureInfo.InvariantCulture),
        mitarbeiter.Skilllevel);
}

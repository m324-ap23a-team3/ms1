namespace Ms1.Api.Contracts;

/// <summary>
/// Request-Body für <c>POST /api/mitarbeiter</c> (US1).
/// Alle Felder sind nullable, damit fehlende Felder im Service mit einer verständlichen Meldung
/// pro Feld abgelehnt werden statt mit einem allgemeinen JSON-Fehler.
/// </summary>
/// <param name="Vorname">Vorname, Pflicht, z. B. «Max».</param>
/// <param name="Nachname">Nachname, Pflicht, z. B. «Muster».</param>
/// <param name="Beitrittsdatum">Firmeneintritt im Format TT.MM.JJJJ (z. B. «15.05.2021») oder JJJJ-MM-TT.</param>
/// <param name="Skilllevel">Skilllevel als Support-Mitarbeiter, ganze Zahl von 1 bis 5.</param>
public record MitarbeiterErfassenRequest(
    string? Vorname,
    string? Nachname,
    string? Beitrittsdatum,
    int? Skilllevel);

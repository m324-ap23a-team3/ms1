namespace Ms1.Api.Contracts;

/// <summary>
/// Antwort von <c>GET /api/mitarbeiter</c> (US2). Immer dieselbe Struktur, auch wenn die Liste leer ist,
/// damit Clients wie MS2 sie einfach auswerten können.
/// </summary>
/// <param name="Anzahl">Anzahl Mitarbeiter in der Liste.</param>
/// <param name="Meldung">Lesbare Zusammenfassung, bei leerer Liste der Hinweis, dass keine Mitarbeiter erfasst sind (AK5).</param>
/// <param name="Mitarbeiter">Alle Mitarbeiter mit allen Feldern, nach ID sortiert.</param>
public record MitarbeiterListeResponse(
    int Anzahl,
    string Meldung,
    IReadOnlyList<MitarbeiterResponse> Mitarbeiter)
{
    public const string MeldungLeer = "Es sind noch keine Mitarbeiter erfasst.";

    public static MitarbeiterListeResponse Aus(IReadOnlyList<MitarbeiterResponse> mitarbeiter) => new(
        mitarbeiter.Count,
        mitarbeiter.Count == 0 ? MeldungLeer : $"{mitarbeiter.Count} Mitarbeiter gefunden.",
        mitarbeiter);
}

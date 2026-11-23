namespace Ms1.Api.Models;

/// <summary>
/// Mitarbeiter im Support, der in MS2 Tickets zugewiesen bekommen kann.
/// </summary>
public class Mitarbeiter
{
    /// <summary>Kleinster erlaubter Skilllevel.</summary>
    public const int SkilllevelMin = 1;

    /// <summary>Grösster erlaubter Skilllevel.</summary>
    public const int SkilllevelMax = 5;

    /// <summary>Maximale Länge von Vor- und Nachname (Spaltenlänge in der DB).</summary>
    public const int NameMaxLaenge = 100;

    /// <summary>Eindeutige ID, wird von der Datenbank automatisch vergeben (US1 AK5).</summary>
    public int Id { get; set; }

    public required string Vorname { get; set; }

    public required string Nachname { get; set; }

    /// <summary>Datum des Firmeneintritts, ohne Uhrzeit.</summary>
    public DateOnly Beitrittsdatum { get; set; }

    /// <summary>Skilllevel als Support-Mitarbeiter, 1 bis 5 (US1 AK6).</summary>
    public int Skilllevel { get; set; }
}

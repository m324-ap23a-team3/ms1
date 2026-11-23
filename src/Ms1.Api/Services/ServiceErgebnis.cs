namespace Ms1.Api.Services;

/// <summary>Ausgang eines Service-Aufrufs.</summary>
public enum ErgebnisArt
{
    /// <summary>Hat geklappt, <see cref="ServiceErgebnis{T}.Wert"/> ist gesetzt.</summary>
    Erfolg,

    /// <summary>Eingaben ungültig, <see cref="ServiceErgebnis{T}.Validierungsfehler"/> enthält die Meldungen pro Feld.</summary>
    Ungueltig,

    /// <summary>Technischer Fehler (z. B. Datenbank nicht erreichbar), <see cref="ServiceErgebnis{T}.Fehlermeldung"/> ist gesetzt.</summary>
    Fehler,
}

/// <summary>
/// Ergebnis der Service-Schicht. Der Service kennt kein HTTP; die Endpoints übersetzen
/// das Ergebnis in Statuscodes (201, 400, 503). So lässt sich die Logik ohne HTTP testen.
/// </summary>
public sealed record ServiceErgebnis<T>
{
    public required ErgebnisArt Art { get; init; }

    public T? Wert { get; init; }

    /// <summary>Feldname (wie im JSON) → Meldungen.</summary>
    public IReadOnlyDictionary<string, string[]> Validierungsfehler { get; init; } = new Dictionary<string, string[]>();

    public string? Fehlermeldung { get; init; }

    public static ServiceErgebnis<T> Erfolg(T wert) => new() { Art = ErgebnisArt.Erfolg, Wert = wert };

    public static ServiceErgebnis<T> Ungueltig(IReadOnlyDictionary<string, string[]> fehler) =>
        new() { Art = ErgebnisArt.Ungueltig, Validierungsfehler = fehler };

    public static ServiceErgebnis<T> Fehler(string meldung) => new() { Art = ErgebnisArt.Fehler, Fehlermeldung = meldung };
}

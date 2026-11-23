using System.Globalization;
using Ms1.Api.Contracts;
using Ms1.Api.Data;
using Ms1.Api.Models;

namespace Ms1.Api.Services;

/// <summary>
/// Geschäftslogik für Mitarbeiter: Validierung und Umwandlung zwischen Request/Response und Entity.
/// Hängt nur von <see cref="IMitarbeiterRepository"/> ab und ist dadurch ohne Datenbank testbar.
/// </summary>
public class MitarbeiterService(IMitarbeiterRepository repository, ILogger<MitarbeiterService> logger)
{
    /// <summary>Akzeptierte Eingabeformate für das Beitrittsdatum.</summary>
    public static readonly string[] DatumsFormate = ["dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd"];

    /// <summary>
    /// US1: Prüft die Eingaben und speichert einen neuen Mitarbeiter. Die ID vergibt die Datenbank.
    /// </summary>
    public async Task<ServiceErgebnis<MitarbeiterResponse>> ErfassenAsync(
        MitarbeiterErfassenRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return ServiceErgebnis<MitarbeiterResponse>.Ungueltig(new Dictionary<string, string[]>
            {
                ["body"] = ["Der Request-Body fehlt. Erwartet wird ein JSON-Objekt mit vorname, nachname, beitrittsdatum und skilllevel."],
            });
        }

        var fehler = new Dictionary<string, string[]>();

        var vorname = PruefeName(request.Vorname, "vorname", "Vorname", fehler);
        var nachname = PruefeName(request.Nachname, "nachname", "Nachname", fehler);
        var beitrittsdatum = PruefeBeitrittsdatum(request.Beitrittsdatum, fehler);
        PruefeSkilllevel(request.Skilllevel, fehler);

        if (fehler.Count > 0)
        {
            return ServiceErgebnis<MitarbeiterResponse>.Ungueltig(fehler);
        }

        var mitarbeiter = new Mitarbeiter
        {
            Vorname = vorname!,
            Nachname = nachname!,
            Beitrittsdatum = beitrittsdatum!.Value,
            Skilllevel = request.Skilllevel!.Value,
        };

        try
        {
            var gespeichert = await repository.HinzufuegenAsync(mitarbeiter, cancellationToken);
            return ServiceErgebnis<MitarbeiterResponse>.Erfolg(MitarbeiterResponse.Aus(gespeichert));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Mitarbeiter konnte nicht gespeichert werden");
            return ServiceErgebnis<MitarbeiterResponse>.Fehler(
                "Der Mitarbeiter konnte wegen eines Datenbankfehlers nicht gespeichert werden. Bitte später erneut versuchen.");
        }
    }

    /// <summary>
    /// US2: Liest alle Mitarbeiter. Eine leere Liste ist kein Fehler, sondern ergibt eine Meldung (AK5).
    /// </summary>
    public async Task<ServiceErgebnis<MitarbeiterListeResponse>> AlleAuslesenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var mitarbeiter = await repository.AlleLesenAsync(cancellationToken);
            var liste = mitarbeiter.Select(MitarbeiterResponse.Aus).ToList();
            return ServiceErgebnis<MitarbeiterListeResponse>.Erfolg(MitarbeiterListeResponse.Aus(liste));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Mitarbeiter konnten nicht geladen werden");
            return ServiceErgebnis<MitarbeiterListeResponse>.Fehler(
                "Die Mitarbeiter konnten wegen eines Datenbankfehlers nicht geladen werden. Bitte später erneut versuchen.");
        }
    }

    private static string? PruefeName(string? wert, string feld, string anzeigename, Dictionary<string, string[]> fehler)
    {
        var name = wert?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            fehler[feld] = [$"{anzeigename} ist ein Pflichtfeld."];
            return null;
        }

        if (name.Length > Mitarbeiter.NameMaxLaenge)
        {
            fehler[feld] = [$"{anzeigename} darf höchstens {Mitarbeiter.NameMaxLaenge} Zeichen lang sein."];
            return null;
        }

        return name;
    }

    private static DateOnly? PruefeBeitrittsdatum(string? wert, Dictionary<string, string[]> fehler)
    {
        if (string.IsNullOrWhiteSpace(wert))
        {
            fehler["beitrittsdatum"] = ["Beitrittsdatum ist ein Pflichtfeld, z. B. «15.05.2021»."];
            return null;
        }

        if (!DateOnly.TryParseExact(wert.Trim(), DatumsFormate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datum))
        {
            fehler["beitrittsdatum"] = [$"Beitrittsdatum «{wert}» ist kein gültiges Datum. Erwartet wird TT.MM.JJJJ, z. B. «15.05.2021»."];
            return null;
        }

        return datum;
    }

    private static void PruefeSkilllevel(int? wert, Dictionary<string, string[]> fehler)
    {
        if (wert is null)
        {
            fehler["skilllevel"] = [$"Skilllevel ist ein Pflichtfeld (ganze Zahl von {Mitarbeiter.SkilllevelMin} bis {Mitarbeiter.SkilllevelMax})."];
        }
        else if (wert is < Mitarbeiter.SkilllevelMin or > Mitarbeiter.SkilllevelMax)
        {
            fehler["skilllevel"] = [$"Skilllevel muss zwischen {Mitarbeiter.SkilllevelMin} und {Mitarbeiter.SkilllevelMax} liegen, war aber {wert}."];
        }
    }
}

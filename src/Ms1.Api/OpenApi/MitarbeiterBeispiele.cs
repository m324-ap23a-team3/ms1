using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Ms1.Api.OpenApi;

/// <summary>
/// Ergänzt die OpenAPI-Beschreibung der Mitarbeiter-Endpunkte um Beispiele für Request und Antworten,
/// inklusive der Fehlerfälle. Grundlage für den MS1-Client in MS2.
/// </summary>
public sealed class MitarbeiterBeispiele : IOpenApiOperationTransformer
{
    private static JsonNode Max() => new JsonObject
    {
        ["id"] = 1,
        ["vorname"] = "Max",
        ["nachname"] = "Muster",
        ["beitrittsdatum"] = "15.05.2021",
        ["skilllevel"] = 4,
    };

    private static JsonNode Anna() => new JsonObject
    {
        ["id"] = 2,
        ["vorname"] = "Anna",
        ["nachname"] = "Beispiel",
        ["beitrittsdatum"] = "31.01.2020",
        ["skilllevel"] = 2,
    };

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        switch (operation.OperationId)
        {
            case "MitarbeiterErfassen":
                BeispieleErfassen(operation);
                break;
            case "MitarbeiterAuslesen":
                BeispieleAuslesen(operation);
                break;
        }

        return Task.CompletedTask;
    }

    private static void BeispieleErfassen(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var request) == true)
        {
            request.Examples = new Dictionary<string, IOpenApiExample>
            {
                ["Spezifikation"] = Beispiel("Beispiel aus der Spezifikation (Datum TT.MM.JJJJ)", new JsonObject
                {
                    ["vorname"] = "Max",
                    ["nachname"] = "Muster",
                    ["beitrittsdatum"] = "15.05.2021",
                    ["skilllevel"] = 4,
                }),
                ["IsoDatum"] = Beispiel("Datum im ISO-Format JJJJ-MM-TT", new JsonObject
                {
                    ["vorname"] = "Anna",
                    ["nachname"] = "Beispiel",
                    ["beitrittsdatum"] = "2020-01-31",
                    ["skilllevel"] = 2,
                }),
                ["Ungueltig"] = Beispiel("Ungültig: ergibt 400", new JsonObject
                {
                    ["vorname"] = "",
                    ["beitrittsdatum"] = "31.02.2021",
                    ["skilllevel"] = 6,
                }),
            };
        }

        SetzeBeispiele(operation, "201", "application/json", new()
        {
            ["Erfasst"] = Beispiel("Mitarbeiter erfasst, ID vergeben", Max()),
        });

        SetzeBeispiele(operation, "400", "application/problem+json", new()
        {
            ["Validierung"] = Beispiel("Meldung pro ungültigem Feld", new JsonObject
            {
                ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                ["title"] = "Die Eingaben sind ungültig.",
                ["status"] = 400,
                ["detail"] = "Bitte die markierten Felder korrigieren.",
                ["errors"] = new JsonObject
                {
                    ["vorname"] = new JsonArray("Vorname ist ein Pflichtfeld."),
                    ["nachname"] = new JsonArray("Nachname ist ein Pflichtfeld."),
                    ["beitrittsdatum"] = new JsonArray("Beitrittsdatum «31.02.2021» ist kein gültiges Datum. Erwartet wird TT.MM.JJJJ, z. B. «15.05.2021»."),
                    ["skilllevel"] = new JsonArray("Skilllevel muss zwischen 1 und 5 liegen, war aber 6."),
                },
            }),
            ["FalscherTyp"] = Beispiel("Falscher Datentyp, z. B. \"skilllevel\": \"vier\"", new JsonObject
            {
                ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                ["title"] = "Die Eingaben sind ungültig.",
                ["status"] = 400,
                ["detail"] = "Der Request konnte nicht gelesen werden. Bitte die markierten Felder korrigieren.",
                ["errors"] = new JsonObject
                {
                    ["skilllevel"] = new JsonArray("Skilllevel muss eine ganze Zahl von 1 bis 5 sein."),
                },
            }),
        });

        SetzeBeispiele(operation, "503", "application/problem+json", new()
        {
            ["DatenbankNichtErreichbar"] = Beispiel("Datenbank nicht erreichbar", Problem503(
                "Der Mitarbeiter konnte wegen eines Datenbankfehlers nicht gespeichert werden. Bitte später erneut versuchen.")),
        });
    }

    private static void BeispieleAuslesen(OpenApiOperation operation)
    {
        SetzeBeispiele(operation, "200", "application/json", new()
        {
            ["MitEintraegen"] = Beispiel("Liste mit Mitarbeitern", new JsonObject
            {
                ["anzahl"] = 2,
                ["meldung"] = "2 Mitarbeiter gefunden.",
                ["mitarbeiter"] = new JsonArray(Max(), Anna()),
            }),
            ["Leer"] = Beispiel("Noch keine Mitarbeiter erfasst", new JsonObject
            {
                ["anzahl"] = 0,
                ["meldung"] = "Es sind noch keine Mitarbeiter erfasst.",
                ["mitarbeiter"] = new JsonArray(),
            }),
        });

        SetzeBeispiele(operation, "503", "application/problem+json", new()
        {
            ["DatenbankNichtErreichbar"] = Beispiel("Datenbank nicht erreichbar", Problem503(
                "Die Mitarbeiter konnten wegen eines Datenbankfehlers nicht geladen werden. Bitte später erneut versuchen.")),
        });
    }

    private static JsonObject Problem503(string detail) => new()
    {
        ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
        ["title"] = "Datenbank nicht verfügbar",
        ["status"] = 503,
        ["detail"] = detail,
    };

    private static OpenApiExample Beispiel(string zusammenfassung, JsonNode wert) => new()
    {
        Summary = zusammenfassung,
        Value = wert,
    };

    private static void SetzeBeispiele(
        OpenApiOperation operation,
        string statusCode,
        string mediaType,
        Dictionary<string, IOpenApiExample> beispiele)
    {
        if (operation.Responses?.TryGetValue(statusCode, out var response) == true
            && response.Content?.TryGetValue(mediaType, out var inhalt) == true)
        {
            inhalt.Examples = beispiele;
        }
    }
}

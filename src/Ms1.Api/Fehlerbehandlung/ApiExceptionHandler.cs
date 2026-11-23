using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ms1.Api.Fehlerbehandlung;

/// <summary>
/// Wandelt Exceptions in verständliche ProblemDetails-Antworten um:
/// <list type="bullet">
/// <item>Ungültiges JSON oder falscher Datentyp (z. B. <c>"skilllevel": "vier"</c>) → 400 mit Meldung pro Feld (US1 AK8)</item>
/// <item>Alles andere → 500 ohne technische Details</item>
/// </list>
/// </summary>
public class ApiExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    private static readonly Dictionary<string, string> MeldungProFeld = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vorname"] = "Vorname muss ein Text sein.",
        ["nachname"] = "Nachname muss ein Text sein.",
        ["beitrittsdatum"] = "Beitrittsdatum muss ein Text im Format TT.MM.JJJJ sein, z. B. «15.05.2021».",
        ["skilllevel"] = "Skilllevel muss eine ganze Zahl von 1 bis 5 sein.",
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is BadHttpRequestException badRequest)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            problem = new HttpValidationProblemDetails(FehlerAus(badRequest))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Die Eingaben sind ungültig.",
                Detail = "Der Request konnte nicht gelesen werden. Bitte die markierten Felder korrigieren.",
            };
        }
        else
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unerwarteter Fehler",
                Detail = "Es ist ein unerwarteter Fehler aufgetreten. Bitte später erneut versuchen.",
            };
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static Dictionary<string, string[]> FehlerAus(BadHttpRequestException exception)
    {
        if (exception.InnerException is JsonException { Path: { } pfad } && pfad.StartsWith("$.", StringComparison.Ordinal))
        {
            var feld = pfad[2..].ToLowerInvariant();
            var meldung = MeldungProFeld.GetValueOrDefault(feld, $"Das Feld {feld} hat einen ungültigen Wert.");
            return new() { [feld] = [meldung] };
        }

        return new() { ["body"] = ["Der Request-Body ist kein gültiges JSON-Objekt."] };
    }
}

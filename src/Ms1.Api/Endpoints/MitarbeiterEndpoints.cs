using Microsoft.AspNetCore.Http.HttpResults;
using Ms1.Api.Contracts;
using Ms1.Api.Services;

namespace Ms1.Api.Endpoints;

/// <summary>
/// HTTP-Schicht für Mitarbeiter. Enthält keine Logik, sondern übersetzt nur
/// zwischen HTTP und <see cref="MitarbeiterService"/>.
/// </summary>
public static class MitarbeiterEndpoints
{
    public const string Route = "/api/mitarbeiter";

    public static IEndpointRouteBuilder MapMitarbeiterEndpoints(this IEndpointRouteBuilder app)
    {
        var gruppe = app.MapGroup(Route).WithTags("Mitarbeiter");

        gruppe.MapPost("", ErfassenAsync)
            .WithName("MitarbeiterErfassen");

        return app;
    }

    /// <summary>US1: Mitarbeiter erfassen.</summary>
    internal static async Task<Results<Created<MitarbeiterResponse>, ValidationProblem, ProblemHttpResult>> ErfassenAsync(
        MitarbeiterErfassenRequest? request,
        MitarbeiterService service,
        CancellationToken cancellationToken)
    {
        var ergebnis = await service.ErfassenAsync(request, cancellationToken);

        return ergebnis.Art switch
        {
            // Kein Location-Header: Es gibt keinen Endpunkt für einen einzelnen Mitarbeiter.
            // Der neue Mitarbeiter erscheint in der Übersicht GET /api/mitarbeiter (US1 AK7, US2).
            ErgebnisArt.Erfolg => TypedResults.Created((string?)null, ergebnis.Wert),
            ErgebnisArt.Ungueltig => TypedResults.ValidationProblem(
                ergebnis.Validierungsfehler.ToDictionary(),
                title: "Die Eingaben sind ungültig.",
                detail: "Bitte die markierten Felder korrigieren."),
            _ => TypedResults.Problem(
                ergebnis.Fehlermeldung,
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Datenbank nicht verfügbar"),
        };
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Ms1.Api.Contracts;

namespace Ms1.Api.Tests.US1;

/// <summary>
/// API-Tests für US1 <c>POST /api/mitarbeiter</c>: prüfen den ganzen HTTP-Weg (Statuscode, JSON-Format,
/// Fehlermeldungen). Die API läuft im Speicher (<see cref="Ms1ApiFactory"/>), die Datenbank ist durch einen Fake ersetzt.
/// </summary>
public class MitarbeiterErfassenApiTests : IClassFixture<Ms1ApiFactory>
{
    private const string Route = "/api/mitarbeiter";
    private readonly Ms1ApiFactory _factory;
    private readonly HttpClient _client;

    public MitarbeiterErfassenApiTests(Ms1ApiFactory factory)
    {
        _factory = factory;
        _factory.Repository.Zuruecksetzen();
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Happy Path (AK7): Ein gültiger Mitarbeiter ergibt <c>201 Created</c>, und die Antwort enthält
    /// den erfassten Mitarbeiter mit der vergebenen ID und allen Feldern.
    /// </summary>
    [Fact]
    public async Task Post_gueltig_liefert_201_mit_dem_Mitarbeiter()
    {
        var response = await _client.PostAsJsonAsync(Route, new
        {
            vorname = "Max",
            nachname = "Muster",
            beitrittsdatum = "15.05.2021",
            skilllevel = 4,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var mitarbeiter = await response.Content.ReadFromJsonAsync<MitarbeiterResponse>();
        Assert.Equal(new MitarbeiterResponse(1, "Max", "Muster", "15.05.2021", 4), mitarbeiter);
    }

    /// <summary>
    /// Sad Path (AK6, AK8): Skilllevel 6 ergibt <c>400 Bad Request</c> als ProblemDetails
    /// mit einer Meldung beim Feld <c>skilllevel</c>. Es wird nichts gespeichert.
    /// </summary>
    [Fact]
    public async Task Post_mit_Skilllevel_6_liefert_400_mit_Meldung_beim_Feld()
    {
        var response = await _client.PostAsJsonAsync(Route, new
        {
            vorname = "Max",
            nachname = "Muster",
            beitrittsdatum = "15.05.2021",
            skilllevel = 6,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("zwischen 1 und 5", Assert.Single(problem!.Errors["skilllevel"]));
        Assert.Empty(_factory.Repository.Gespeichert);
    }

    /// <summary>
    /// Sad Path (AK8): Fehlen alle Felder, enthält die 400-Antwort für jedes Pflichtfeld eine Meldung.
    /// </summary>
    [Fact]
    public async Task Post_leeres_Objekt_liefert_400_mit_allen_Pflichtfeldern()
    {
        var response = await _client.PostAsJsonAsync(Route, new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["beitrittsdatum", "nachname", "skilllevel", "vorname"], problem!.Errors.Keys.Order());
    }

    /// <summary>
    /// Sad Path (AK8): Ein falscher Datentyp (Skilllevel als Text) ergibt 400 mit einer Meldung
    /// beim betroffenen Feld statt eines allgemeinen Fehlers.
    /// </summary>
    [Fact]
    public async Task Post_mit_Skilllevel_als_Text_liefert_400_mit_Meldung_beim_Feld()
    {
        var json = """{"vorname":"Max","nachname":"Muster","beitrittsdatum":"15.05.2021","skilllevel":"vier"}""";

        var response = await _client.PostAsync(Route, new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["Skilllevel muss eine ganze Zahl von 1 bis 5 sein."], problem!.Errors["skilllevel"]);
    }

    /// <summary>
    /// Sad Path (AK4): Auch eine Zahl in Anführungszeichen (<c>"skilllevel": "4"</c>) wird abgelehnt,
    /// weil der Skilllevel als JSON-Zahl erwartet wird (so steht es auch in der OpenAPI-Beschreibung).
    /// </summary>
    [Fact]
    public async Task Post_mit_Skilllevel_als_Zahl_in_Anfuehrungszeichen_liefert_400()
    {
        var json = """{"vorname":"Max","nachname":"Muster","beitrittsdatum":"15.05.2021","skilllevel":"4"}""";

        var response = await _client.PostAsync(Route, new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.True(problem!.Errors.ContainsKey("skilllevel"));
    }

    /// <summary>
    /// Sad Path (AK8): Ungültiges JSON ergibt 400 mit einer Meldung beim Feld <c>body</c>.
    /// </summary>
    [Fact]
    public async Task Post_mit_ungueltigem_JSON_liefert_400()
    {
        var response = await _client.PostAsync(Route, new StringContent("{kaputt", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.True(problem!.Errors.ContainsKey("body"));
    }

    /// <summary>
    /// Sad Path: Ist die Datenbank nicht erreichbar, antwortet die API mit <c>503</c> und einer
    /// verständlichen Meldung, ohne technische Details preiszugeben.
    /// </summary>
    [Fact]
    public async Task Post_bei_Datenbankfehler_liefert_503()
    {
        _factory.Repository.Fehler = new InvalidOperationException("Connection refused");

        var response = await _client.PostAsJsonAsync(Route, new
        {
            vorname = "Max",
            nachname = "Muster",
            beitrittsdatum = "15.05.2021",
            skilllevel = 4,
        });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("nicht gespeichert", problem!.Detail);
        Assert.DoesNotContain("Connection refused", problem.Detail);
    }
}

using System.Net;
using System.Text.Json.Nodes;

namespace Ms1.Api.Tests;

/// <summary>
/// Prüft die OpenAPI-Beschreibung unter <c>/openapi/v1.json</c> (ms1#8). Sie ist die Grundlage
/// für den MS1-Client in MS2 und muss beide Endpunkte mit Beispielen enthalten.
/// </summary>
public class OpenApiTests(Ms1ApiFactory factory) : IClassFixture<Ms1ApiFactory>
{
    private async Task<JsonNode> LadeDokumentAsync()
    {
        var response = await factory.CreateClient().GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    /// <summary>
    /// Happy Path: POST (US1) und GET (US2) auf <c>/api/mitarbeiter</c> sind mit Zusammenfassung beschrieben.
    /// </summary>
    [Fact]
    public async Task Beide_Endpunkte_sind_beschrieben()
    {
        var dokument = await LadeDokumentAsync();

        var pfad = dokument["paths"]!["/api/mitarbeiter"]!;
        Assert.StartsWith("US1", pfad["post"]!["summary"]!.GetValue<string>());
        Assert.StartsWith("US2", pfad["get"]!["summary"]!.GetValue<string>());
    }

    /// <summary>
    /// Happy Path: Für jeden Statuscode beider Endpunkte gibt es mindestens ein Beispiel,
    /// auch für die Fehlerfälle 400 und 503.
    /// </summary>
    [Theory]
    [InlineData("post", "201", "application/json")]
    [InlineData("post", "400", "application/problem+json")]
    [InlineData("post", "503", "application/problem+json")]
    [InlineData("get", "200", "application/json")]
    [InlineData("get", "503", "application/problem+json")]
    public async Task Jede_Antwort_hat_ein_Beispiel(string methode, string statusCode, string mediaType)
    {
        var dokument = await LadeDokumentAsync();

        var beispiele = dokument["paths"]!["/api/mitarbeiter"]![methode]!["responses"]![statusCode]!["content"]![mediaType]!["examples"];

        Assert.NotNull(beispiele);
        Assert.NotEmpty(beispiele.AsObject());
    }

    /// <summary>
    /// Happy Path: Der Request-Body von POST hat Beispiele, darunter das aus der Spezifikation (Max Muster, 15.05.2021).
    /// </summary>
    [Fact]
    public async Task Request_hat_Beispiel_aus_der_Spezifikation()
    {
        var dokument = await LadeDokumentAsync();

        var beispiel = dokument["paths"]!["/api/mitarbeiter"]!["post"]!["requestBody"]!["content"]!["application/json"]!["examples"]!["Spezifikation"]!["value"]!;

        Assert.Equal("15.05.2021", beispiel["beitrittsdatum"]!.GetValue<string>());
        Assert.Equal(4, beispiel["skilllevel"]!.GetValue<int>());
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Ms1.Api.Contracts;

namespace Ms1.Api.Tests.US2;

/// <summary>
/// API-Tests für US2 <c>GET /api/mitarbeiter</c>: ganzer HTTP-Weg mit <see cref="Ms1ApiFactory"/>,
/// die Datenbank ist durch einen Fake ersetzt.
/// </summary>
public class MitarbeiterAuslesenApiTests : IClassFixture<Ms1ApiFactory>
{
    private const string Route = "/api/mitarbeiter";
    private readonly Ms1ApiFactory _factory;
    private readonly HttpClient _client;

    public MitarbeiterAuslesenApiTests(Ms1ApiFactory factory)
    {
        _factory = factory;
        _factory.Repository.Zuruecksetzen();
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Happy Path (US1 AK7, US2 AK1–3): Ein per POST erfasster Mitarbeiter erscheint danach in der
    /// Übersicht <c>GET /api/mitarbeiter</c> mit allen Feldern.
    /// </summary>
    [Fact]
    public async Task Erfasster_Mitarbeiter_erscheint_in_der_Uebersicht()
    {
        var post = await _client.PostAsJsonAsync(Route, new
        {
            vorname = "Max",
            nachname = "Muster",
            beitrittsdatum = "15.05.2021",
            skilllevel = 4,
        });
        var erfasst = await post.Content.ReadFromJsonAsync<MitarbeiterResponse>();

        var response = await _client.GetAsync(Route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var liste = await response.Content.ReadFromJsonAsync<MitarbeiterListeResponse>();
        Assert.Equal(1, liste!.Anzahl);
        Assert.Equal(erfasst, Assert.Single(liste.Mitarbeiter));
    }

    /// <summary>
    /// Randfall (AK5): Ohne erfasste Mitarbeiter antwortet die API mit <c>200</c>, einer leeren Liste
    /// und der Meldung, dass noch keine Mitarbeiter erfasst sind.
    /// </summary>
    [Fact]
    public async Task Leere_Liste_liefert_200_mit_Meldung()
    {
        var response = await _client.GetAsync(Route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var liste = await response.Content.ReadFromJsonAsync<MitarbeiterListeResponse>();
        Assert.Equal(0, liste!.Anzahl);
        Assert.Equal("Es sind noch keine Mitarbeiter erfasst.", liste.Meldung);
        Assert.Empty(liste.Mitarbeiter);
    }

    /// <summary>
    /// Happy Path (AK2, Format für MS2): Die JSON-Antwort hat die Felder <c>anzahl</c>, <c>meldung</c>
    /// und <c>mitarbeiter</c>, jeder Mitarbeiter <c>id</c>, <c>vorname</c>, <c>nachname</c>,
    /// <c>beitrittsdatum</c> und <c>skilllevel</c>. MS2 verlässt sich auf diese Namen.
    /// </summary>
    [Fact]
    public async Task Antwort_hat_die_vereinbarten_JSON_Felder()
    {
        await _client.PostAsJsonAsync(Route, new
        {
            vorname = "Max",
            nachname = "Muster",
            beitrittsdatum = "15.05.2021",
            skilllevel = 4,
        });

        var json = await _client.GetStringAsync(Route);

        Assert.Equal(
            """{"anzahl":1,"meldung":"1 Mitarbeiter gefunden.","mitarbeiter":[{"id":1,"vorname":"Max","nachname":"Muster","beitrittsdatum":"15.05.2021","skilllevel":4}]}""",
            json);
    }

    /// <summary>
    /// Sad Path (AK6): Ist die Datenbank nicht erreichbar, antwortet die API mit <c>503</c> und
    /// einer klaren Meldung als ProblemDetails.
    /// </summary>
    [Fact]
    public async Task Fehler_beim_Laden_liefert_503_mit_Meldung()
    {
        _factory.Repository.Fehler = new InvalidOperationException("Connection refused");

        var response = await _client.GetAsync(Route);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("nicht geladen", problem!.Detail);
    }

    /// <summary>
    /// Sad Path (AK6): Eine nicht unterstützte Methode auf der Route liefert <c>405</c> mit ProblemDetails
    /// statt einer leeren Antwort.
    /// </summary>
    [Fact]
    public async Task Falsche_Methode_liefert_405_mit_ProblemDetails()
    {
        var response = await _client.DeleteAsync(Route);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}

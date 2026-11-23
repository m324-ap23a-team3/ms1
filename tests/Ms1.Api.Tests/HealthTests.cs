using System.Net;

namespace Ms1.Api.Tests;

public class HealthTests(Ms1ApiFactory factory) : IClassFixture<Ms1ApiFactory>
{
    /// <summary>
    /// Der Health-Endpunkt antwortet mit 200, auch ohne laufende Datenbank.
    /// </summary>
    [Fact]
    public async Task Health_liefert_200()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

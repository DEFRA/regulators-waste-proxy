using System.Net;
using AwesomeAssertions;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests;

[Collection(nameof(WebApplicationFactoryCollection))]
public class FaviconTests(ReverseProxyWebApplicationFactory factory) : IClassFixture<ReverseProxyWebApplicationFactory>
{
    [Fact]
    public async Task GetFavicon_ShouldReturnNoContent()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/favicon.ico", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

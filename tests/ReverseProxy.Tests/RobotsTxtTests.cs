using System.Net;
using AwesomeAssertions;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests;

[Collection(nameof(WebApplicationFactoryCollection))]
public class RobotsTxtTests(ReverseProxyWebApplicationFactory factory)
    : IClassFixture<ReverseProxyWebApplicationFactory>
{
    [Fact]
    public async Task GetRobotsTxt_ShouldDisallowAllCrawlers()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/robots.txt", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Be("User-agent: *\nDisallow: /\n");
    }

    [Fact]
    public async Task HeadRobotsTxt_ShouldReturnOk()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, "/robots.txt");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
    }
}

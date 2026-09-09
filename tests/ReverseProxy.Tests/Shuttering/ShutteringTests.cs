using System.Net;
using AwesomeAssertions;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests.Shuttering;

[Collection(nameof(WebApplicationFactoryCollection))]
public class ShutteringTests(ShutteredReverseProxyWebApplicationFactory factory)
    : IClassFixture<ShutteredReverseProxyWebApplicationFactory>
{
    [Theory]
    [InlineData("/certificates-of-compliance")]
    [InlineData("/certificates-of-compliance/returns")]
    [InlineData("/certificates-of-compliance/assets/application.css")]
    [InlineData("/certificates-of-compliance/pages/a-nested-page.html")]
    public async Task GetRequestToShutteredPathAndSuffix_ShouldReturnHoldingPageWithConfiguredHtmlBody(string path)
    {
        factory.ShutteringMetrics.Reset();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        response.Headers.CacheControl!.ToString().Should().Be("no-store");

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.Should().Contain("<title>Service Unavailable</title>");
        content.Should().Contain("<h1 class=\"govuk-heading-l\">Sorry, the service is unavailable</h1>");
        content.Should().Contain("https://www.gov.uk/guidance/contact-defra");
        content.Should().Contain("/govuk-frontend.min.css");
        content.Should().Contain("class=\"defra-internal-header\"");
        content.Should().Contain("Department for Environment,");
        content.Should().Contain("class=\"govuk-footer__licence-logo\"");
        content.Should().Contain("Open Government Licence v3.0");
        content.Should().Contain("© Crown copyright");
        content.Should().Contain("eprcustomerservice@defra.gov.uk");
        content.Should().Contain("0300 060 0002");
        factory
            .ShutteringMetrics.RouteIds.Should()
            .ContainSingle()
            .Which.Should()
            .Be("RegulatorsCertificatesOfCompliance");
    }

    [Fact]
    public async Task PostRequestToShutteredPath_ShouldReturnHoldingPage()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/certificates-of-compliance/returns",
            new StringContent("{}"),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task RequestToGovUkStylesheet_ShouldReturnStylesheet()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/govuk-frontend.min.css", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/css");

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.Should().Contain("--govuk-frontend-version:\"5.13.0\"");
    }

    [Fact]
    public async Task RequestToHealth_ShouldNotBeShuttered()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

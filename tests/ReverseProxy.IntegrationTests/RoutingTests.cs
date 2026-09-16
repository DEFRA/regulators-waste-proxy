using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.IntegrationTests;

public class RoutingTests : IntegrationTestBase
{
    private const string TraceId = "4d2b9f4e-24de-467a-951f-342579445b2a";

    [Fact]
    public async Task RegulatorsCertificatesOfCompliance_ShouldRemovePublicPrefixAndForwardIt()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/certificates-of-compliance/returns?year=2026",
            new { reference = "example" },
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var downstreamRequest = await response.Content.ReadFromJsonAsync<DownstreamRequest>(
            TestContext.Current.CancellationToken
        );

        downstreamRequest.Should().NotBeNull();
        downstreamRequest.Method.Should().Be(HttpMethod.Post.Method);
        downstreamRequest.Path.Should().Be("/returns");
        downstreamRequest.Query.Should().Be("?year=2026");
    }

    [Fact]
    public async Task RegulatorsCertificatesOfCompliance_WhenTraceHeaderReceived_ShouldForwardTraceHeader()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Add(TraceHeaderName, TraceId);

        var response = await client.PostAsJsonAsync(
            "/certificates-of-compliance/trace-returns?year=2026",
            new { reference = "example" },
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var downstreamRequest = await response.Content.ReadFromJsonAsync<DownstreamRequest>(
            TestContext.Current.CancellationToken
        );

        downstreamRequest.Should().NotBeNull();
        downstreamRequest.CorrelationId.Should().Be(TraceId);
    }

    [Fact]
    public async Task RegulatorsCertificatesOfComplianceRoot_WhenExactPathRequested_ShouldRewritePathToRoot()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/certificates-of-compliance", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var downstreamRequest = await response.Content.ReadFromJsonAsync<DownstreamRequest>(
            TestContext.Current.CancellationToken
        );

        downstreamRequest.Should().NotBeNull();
        downstreamRequest.Method.Should().Be(HttpMethod.Get.Method);
        downstreamRequest.Path.Should().Be("/");
    }

    [Fact]
    public async Task RegulatorsWasteDashboard_ShouldRemovePublicPrefixAndForwardIt()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/dashboard/home", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var downstreamRequest = await response.Content.ReadFromJsonAsync<DownstreamRequest>(
            TestContext.Current.CancellationToken
        );

        downstreamRequest.Should().NotBeNull();
        downstreamRequest.Method.Should().Be(HttpMethod.Get.Method);
        downstreamRequest.Path.Should().Be("/home");
    }

    [Fact]
    public async Task RegulatorsWasteDashboardRoot_WhenDashboardRootRequested_ShouldRewritePathToHome()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/dashboard", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var downstreamRequest = await response.Content.ReadFromJsonAsync<DownstreamRequest>(
            TestContext.Current.CancellationToken
        );

        downstreamRequest.Should().NotBeNull();
        downstreamRequest.Method.Should().Be(HttpMethod.Get.Method);
        downstreamRequest.Path.Should().Be("/home");
    }

    [Fact]
    public async Task RootPath_ShouldRedirectToDashboard()
    {
        using var client = new HttpClient(
            new HttpClientHandler { AllowAutoRedirect = false }
        ) { BaseAddress = new Uri("http://localhost:8085") };

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Headers.Location.Should().Be(new Uri("/dashboard", UriKind.Relative));
    }

    [Fact]
    public async Task ShutteredProxy_ShouldReturnItsMountedHoldingPageInsteadOfProxying()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(
            "/shuttered-proxy/pages/a-nested-page.html",
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Headers.Location.Should().BeNull();
        response.Headers.CacheControl!.ToString().Should().Be("no-store");

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.Should().Contain("This path is shuttered for integration testing");
        content.Should().Contain("not part of the proxy image");
    }

    private sealed record DownstreamRequest(string Method, string? Path, string? Query, string? CorrelationId);
}

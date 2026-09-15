using System.Net;
using AwesomeAssertions;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests;

public class ProgramTests
{
    [Fact]
    public async Task WhenRunningAsContainerInDevelopment_AppStartsSuccessfully()
    {
        // Reproduces the Docker compose scenario: ASPNETCORE_ENVIRONMENT=Development
        // and DOTNET_RUNNING_IN_CONTAINER=true. The app must bind plain HTTP — there
        // is no developer SSL certificate inside a container.
        using var factory = new ContainerDevelopmentReverseProxyWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WhenPortIsNotConfigured_AppStartsSuccessfully()
    {
        // When PORT is absent the Kestrel override block is skipped and the app falls
        // back to the framework defaults without throwing.
        using var factory = new NoPortReverseProxyWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WhenPortIsNotAnInteger_AppStartsSuccessfully()
    {
        // When PORT cannot be parsed as an integer int.TryParse returns false and the
        // Kestrel override is silently skipped; the app still starts on default ports.
        using var factory = new InvalidPortReverseProxyWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

using Defra.RegulatorsWasteProxy.ReverseProxy.Utils.Metrics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests;

[CollectionDefinition(nameof(WebApplicationFactoryCollection), DisableParallelization = true)]
public sealed class WebApplicationFactoryCollection;

public sealed class ReverseProxyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
    }
}

public sealed class InvalidConfigurationReverseProxyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("InvalidConfiguration");
    }
}

public sealed class ShutteredReverseProxyWebApplicationFactory : WebApplicationFactory<Program>
{
    public ShutteringMetricsSpy ShutteringMetrics { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("ShutteringTests");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IShutteringMetrics>();
            services.AddSingleton<IShutteringMetrics>(ShutteringMetrics);
        });
    }
}

/// <summary>
/// Simulates the Docker compose scenario: Development environment with DOTNET_RUNNING_IN_CONTAINER=true.
/// The app must start using plain HTTP — no developer SSL certificate is present in a container.
/// </summary>
public sealed class ContainerDevelopmentReverseProxyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["DOTNET_RUNNING_IN_CONTAINER"] = "true",
                    ["PORT"] = "0",
                    ["AWS_EMF_ENABLED"] = "false",
                    ["Health:All:ApiKey"] = "ApiKey",
                }
            );
        });
    }
}

/// <summary>
/// No PORT configuration — the Kestrel override block should be skipped and the app
/// should fall back to the framework defaults without throwing.
/// </summary>
public sealed class NoPortReverseProxyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?> { ["PORT"] = null });
        });
    }
}

/// <summary>
/// PORT set to a non-integer value — TryParse should fail silently and the app should
/// start using the framework defaults.
/// </summary>
public sealed class InvalidPortReverseProxyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?> { ["PORT"] = "not-a-port" });
        });
    }
}

public sealed class ShutteringMetricsSpy : IShutteringMetrics
{
    public List<string> RouteIds { get; } = [];

    public void Reset() => RouteIds.Clear();

    public void ResponseReturned(string routeId)
    {
        RouteIds.Add(routeId);
    }
}

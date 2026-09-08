using System.Diagnostics.CodeAnalysis;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Utils.Metrics;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShutteringMetrics(this IServiceCollection services)
    {
        services.AddMetrics();
        services.AddSingleton<IShutteringMetrics, ShutteringMetrics>();

        return services;
    }
}

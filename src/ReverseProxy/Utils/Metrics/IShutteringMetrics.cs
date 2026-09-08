namespace Defra.RegulatorsWasteProxy.ReverseProxy.Utils.Metrics;

public interface IShutteringMetrics
{
    void ResponseReturned(string routeId);
}

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Utils.Shuttering;

internal sealed record ShutteredRoute(string RouteId, string ClusterId, string MatchPath);

using System.ComponentModel.DataAnnotations;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Utils.Logging;

public class TraceHeader
{
    [ConfigurationKeyName("TraceHeader")]
    [Required]
    public required string Name { get; set; }
}

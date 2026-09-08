using AwesomeAssertions;
using Defra.RegulatorsWasteProxy.ReverseProxy.Configuration;
using Microsoft.Extensions.Configuration;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests.Configuration;

public class ReverseProxyConfigurationValidatorTests
{
    [Fact]
    public void Validate_WhenDestinationAddressIsUnconfigured_ShouldThrow()
    {
        var configuration = CreateConfiguration("https://unconfigured.invalid/");

        var act = () => ReverseProxyConfigurationValidator.Validate(configuration.GetSection("ReverseProxy"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*RegulatorsCertificatesOfCompliance:Primary*");
    }

    [Fact]
    public void Validate_WhenDestinationAddressIsConfigured_ShouldNotThrow()
    {
        var configuration = CreateConfiguration("https://certificates-of-compliance.example/");

        var act = () => ReverseProxyConfigurationValidator.Validate(configuration.GetSection("ReverseProxy"));

        act.Should().NotThrow();
    }

    private static IConfiguration CreateConfiguration(string address) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ReverseProxy:Clusters:RegulatorsCertificatesOfCompliance:Destinations:Primary:Address"] = address,
                }
            )
            .Build();
}

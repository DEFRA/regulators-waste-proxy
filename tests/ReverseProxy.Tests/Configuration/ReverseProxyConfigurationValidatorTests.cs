using AwesomeAssertions;
using Defra.RegulatorsWasteProxy.ReverseProxy.Configuration;
using Microsoft.Extensions.Configuration;

namespace Defra.RegulatorsWasteProxy.ReverseProxy.Tests.Configuration;

public class ReverseProxyConfigurationValidatorTests
{
    [Fact]
    public void Validate_WhenCertificatesOfComplianceDestinationIsUnconfigured_ShouldThrow()
    {
        var configuration = CreateConfiguration(
            dashboardAddress: "https://dashboard.example/",
            certificatesAddress: "https://unconfigured.invalid/"
        );

        var act = () => ReverseProxyConfigurationValidator.Validate(configuration.GetSection("ReverseProxy"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*RegulatorsCertificatesOfCompliance:Primary*");
    }

    [Fact]
    public void Validate_WhenDashboardDestinationIsUnconfigured_ShouldThrow()
    {
        var configuration = CreateConfiguration(
            dashboardAddress: "https://unconfigured.invalid/",
            certificatesAddress: "https://certificates-of-compliance.example/"
        );

        var act = () => ReverseProxyConfigurationValidator.Validate(configuration.GetSection("ReverseProxy"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*RegulatorsWasteDashboard:Primary*");
    }

    [Fact]
    public void Validate_WhenAllDestinationAddressesAreConfigured_ShouldNotThrow()
    {
        var configuration = CreateConfiguration(
            dashboardAddress: "https://dashboard.example/",
            certificatesAddress: "https://certificates-of-compliance.example/"
        );

        var act = () => ReverseProxyConfigurationValidator.Validate(configuration.GetSection("ReverseProxy"));

        act.Should().NotThrow();
    }

    private static IConfiguration CreateConfiguration(string dashboardAddress, string certificatesAddress) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ReverseProxy:Clusters:RegulatorsWasteDashboard:Destinations:Primary:Address"] = dashboardAddress,
                    ["ReverseProxy:Clusters:RegulatorsCertificatesOfCompliance:Destinations:Primary:Address"] = certificatesAddress,
                }
            )
            .Build();
}

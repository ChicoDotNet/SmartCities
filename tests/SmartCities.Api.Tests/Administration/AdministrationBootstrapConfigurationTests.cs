using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Administration;
using Xunit;

namespace SmartCities.Api.Tests.Administration;

public sealed class AdministrationBootstrapConfigurationTests
{
  [Fact]
  public void Short_configured_bootstrap_password_fails_fast()
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(
        new Dictionary<string, string?>
        {
          ["SmartCities:Administration:Bootstrap:Password"] =
            "too-short",
        })
      .Build();
    var services = new ServiceCollection();

    var exception =
      Assert.Throws<InvalidOperationException>(
        () => services
          .AddSmartCitiesAdministrationBootstrap(
            configuration));

    Assert.Contains(
      "at least 16",
      exception.Message,
      StringComparison.Ordinal);
  }

  [Fact]
  public void Missing_bootstrap_password_keeps_bootstrap_login_disabled_without_breaking_host_composition()
  {
    var services = new ServiceCollection();

    services.AddSmartCitiesAdministrationBootstrap(
      new ConfigurationBuilder().Build());

    using var provider = services.BuildServiceProvider();

    Assert.NotNull(
      provider.GetRequiredService<
        AdministrationBootstrapConfiguration>());
  }
}

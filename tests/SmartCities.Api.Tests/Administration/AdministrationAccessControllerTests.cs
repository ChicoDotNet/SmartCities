using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Administration;
using SmartCities.Application.Administration;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Administration;

public sealed class AdministrationAccessControllerTests
{
  [Fact]
  public async Task Anonymous_access_probe_reports_bootstrap_availability_without_granting_access()
  {
    var controller = CreateController(
      new RecordingAdministrationAccessService(
        isAuthorized: false,
        bootstrapAvailable: true),
      authenticated: false,
      bootstrapConfigured: true);

    var result = await controller.GetAccessAsync(
      TestContext.Current.CancellationToken);

    var payload = Assert.IsType<AdministrationAccessResponse>(
      Assert.IsType<OkObjectResult>(result.Result).Value);

    Assert.False(payload.Authorized);
    Assert.True(payload.BootstrapAvailable);
  }

  [Fact]
  public async Task Empty_whitelist_does_not_advertise_bootstrap_when_the_deployment_secret_is_absent()
  {
    var controller = CreateController(
      new RecordingAdministrationAccessService(
        isAuthorized: false,
        bootstrapAvailable: true),
      authenticated: false,
      bootstrapConfigured: false);

    var result = await controller.GetAccessAsync(
      TestContext.Current.CancellationToken);

    var payload = Assert.IsType<AdministrationAccessResponse>(
      Assert.IsType<OkObjectResult>(result.Result).Value);

    Assert.False(payload.Authorized);
    Assert.False(payload.BootstrapAvailable);
  }

  [Fact]
  public async Task Canonical_whitelisted_identity_is_admitted()
  {
    var controller = CreateController(
      new RecordingAdministrationAccessService(
        isAuthorized: true,
        bootstrapAvailable: false),
      authenticated: true,
      bootstrapConfigured: true);

    var result = await controller.GetAccessAsync(
      TestContext.Current.CancellationToken);

    var payload = Assert.IsType<AdministrationAccessResponse>(
      Assert.IsType<OkObjectResult>(result.Result).Value);

    Assert.True(payload.Authorized);
    Assert.False(payload.BootstrapAvailable);
  }

  [Fact]
  public void Whitelist_mutation_requires_its_canonical_management_policy()
  {
    var method = typeof(AdministrationWhitelistController)
      .GetMethod(nameof(AdministrationWhitelistController.AddAsync));

    var authorize = Assert.Single(
      method!.GetCustomAttributes(
        typeof(AuthorizeAttribute),
        inherit: true)
      .Cast<AuthorizeAttribute>());

    Assert.Equal(
      SmartCitiesPolicies.ManageAdministrationWhitelist,
      authorize.Policy);
  }

  private static AdministrationAccessController CreateController(
    IAdministrationAccessService service,
    bool authenticated,
    bool bootstrapConfigured)
  {
    var configurationValues =
      new Dictionary<string, string?>();

    if (bootstrapConfigured)
    {
      configurationValues[
        "SmartCities:Administration:Bootstrap:Password"] =
        "test-bootstrap-password";
    }

    var services = new ServiceCollection();
    services.AddSmartCitiesAdministrationBootstrap(
      new ConfigurationBuilder()
        .AddInMemoryCollection(configurationValues)
        .Build());

    using var provider =
      services.BuildServiceProvider();
    var bootstrap = provider.GetRequiredService<
      AdministrationBootstrapConfiguration>();

    var controller =
      new AdministrationAccessController(
        service,
        bootstrap);
    var context = new DefaultHttpContext();

    if (authenticated)
    {
      context.User = new ClaimsPrincipal(
        new ClaimsIdentity(
          [
            new Claim(
              SmartCitiesClaimTypes.Subject,
              "provider-a:tenant-a:official-001"),
            new Claim(
              SmartCitiesClaimTypes.IdentityProvider,
              "provider-a"),
            new Claim(
              SmartCitiesClaimTypes.EmailAddress,
              "official@townhall.gov"),
          ],
          authenticationType: "provider-a"));
    }

    controller.ControllerContext =
      new ControllerContext
      {
        HttpContext = context,
      };

    return controller;
  }

  private sealed class RecordingAdministrationAccessService
    : IAdministrationAccessService
  {
    private readonly bool isAuthorized;
    private readonly bool bootstrapAvailable;

    public RecordingAdministrationAccessService(
      bool isAuthorized,
      bool bootstrapAvailable)
    {
      this.isAuthorized = isAuthorized;
      this.bootstrapAvailable = bootstrapAvailable;
    }

    public string TownHallId => "town-hall-a";

    public Task<bool> IsAuthorizedAsync(
      AdministrationIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(isAuthorized);
    }

    public Task<bool> IsBootstrapAvailableAsync(
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(bootstrapAvailable);
    }

    public Task<IReadOnlyList<AdministrationAccessRule>> GetRulesAsync(
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<AdministrationAccessRule> AddRuleAsync(
      AdministrationAccessRuleKind kind,
      string value,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<bool> DeleteRuleAsync(
      string ruleId,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }
}

using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Identity;
using SmartCities.Application.Administration;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Identity;

public sealed class PersistentAuthorizationGrantPipelineTests
{
  [Fact]
  public async Task Persisted_grants_are_added_after_provider_canonicalization_before_authorization()
  {
    await using var app = await StartAsync(
      new RecordingGrantService(
        AdministrationEffectiveGrants.Create(
          ["mobility-reviewer"],
          [
            SmartCitiesPermissions.ManageFeatureFlags,
            SmartCitiesFeaturePermissions.Manage(
              "citizen-mobility"),
          ])));

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/managed-mobility",
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.OK,
      response.StatusCode);
  }

  [Fact]
  public async Task Revoked_persisted_grants_are_not_retained_in_the_next_request()
  {
    var grants = new RecordingGrantService(
      AdministrationEffectiveGrants.Create(
        ["mobility-reviewer"],
        [
          SmartCitiesPermissions.ManageFeatureFlags,
          SmartCitiesFeaturePermissions.Manage(
            "citizen-mobility"),
        ]));
    await using var app = await StartAsync(grants);

    using var client = app.GetTestClient();

    using var first = await client.GetAsync(
      "/managed-mobility",
      TestContext.Current.CancellationToken);
    Assert.Equal(
      HttpStatusCode.OK,
      first.StatusCode);

    grants.Effective =
      AdministrationEffectiveGrants.Empty;

    using var second = await client.GetAsync(
      "/managed-mobility",
      TestContext.Current.CancellationToken);
    Assert.Equal(
      HttpStatusCode.Forbidden,
      second.StatusCode);
  }

  private static async Task<WebApplication> StartAsync(
    IAdministrationAuthorizationGrantService grants)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Services.AddLogging();
    builder.Services.AddSmartCitiesAuthenticationCanonicalization();
    builder.Services.AddSmartCitiesAuthorization();
    builder.Services.AddSingleton(grants);
    builder.Services.AddSingleton<IAuthenticationProviderAdapter>(
      new TestProviderAdapter());

    var app = builder.Build();

    app.UseAuthentication();
    app.Use(
      async (context, next) =>
      {
        context.User = new ClaimsPrincipal(
          new ClaimsIdentity(
            [
              new Claim("sub", "official-1"),
            ],
            authenticationType: "Bearer"));

        context.SetValidatedExternalAuthentication(
          AuthenticationProviderContext.Create(
            "Bearer",
            "https://idp.example",
            "tenant-a"),
          ExternalAuthenticatedIdentity.Create(
            "official-1",
            [
              ExternalIdentityClaim.Create(
                "email",
                "official@townhall.gov"),
            ]));

        await next(context);
      });

    app.UseSmartCitiesAuthenticationCanonicalization();
    app.UseAuthorization();

    app.MapGet(
        "/managed-mobility",
        static () => Results.Ok())
      .RequireAuthorization(
        SmartCitiesPolicies.ManageCitizenMobility);

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    return app;
  }

  private sealed class RecordingGrantService(
    AdministrationEffectiveGrants effective)
    : IAdministrationAuthorizationGrantService
  {
    public AdministrationEffectiveGrants Effective { get; set; } =
      effective;

    public string TownHallId => "town-hall-a";

    public Task<AdministrationEffectiveGrants> GetEffectiveAsync(
      AdministrationIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(Effective);
    }

    public Task<IReadOnlyList<AdministrationAuthorizationGrant>>
      GetAllAsync(
        CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<AdministrationAuthorizationGrant> AddAsync(
      AdministrationAccessRuleKind targetKind,
      string targetValue,
      AdministrationAuthorizationGrantKind kind,
      string value,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<bool> DeleteAsync(
      string grantId,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }

  private sealed class TestProviderAdapter
    : IAuthenticationProviderAdapter
  {
    public string ProviderId => "provider-a";

    public bool CanHandle(
      AuthenticationProviderContext context) =>
      true;

    public Task<CanonicalIdentity> NormalizeAsync(
      AuthenticationProviderContext context,
      ExternalAuthenticatedIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        CanonicalIdentity.Create(
          "provider-a",
          "provider-a:tenant-a:official-1",
          [],
          [],
          "official@townhall.gov"));
    }
  }
}

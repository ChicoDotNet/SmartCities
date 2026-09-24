using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Identity;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Identity;

public sealed class AuthenticationCanonicalizationPipelineTests
{
  [Fact]
  public async Task Registration_adds_authentication_services_without_selecting_a_default_provider()
  {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSmartCitiesAuthenticationCanonicalization();

    using var provider = services.BuildServiceProvider();
    var schemes = provider.GetRequiredService<
      IAuthenticationSchemeProvider>();

    var defaultScheme =
      await schemes.GetDefaultAuthenticateSchemeAsync();

    Assert.Null(defaultScheme);
  }

  [Fact]
  public async Task No_configured_provider_challenges_protected_routes_with_401_instead_of_failing_host_execution()
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Services.AddSmartCitiesAuthenticationCanonicalization();
    builder.Services.AddSmartCitiesAuthorization();

    await using var app = builder.Build();

    app.UseAuthentication();
    app.UseSmartCitiesAuthenticationCanonicalization();
    app.UseAuthorization();

    app.MapGet(
        "/protected-without-provider",
        static () => Results.Ok())
      .RequireAuthorization(
        SmartCitiesPolicies.FinalizeDecisionReview);

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/protected-without-provider",
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.Unauthorized,
      response.StatusCode);
  }

  [Fact]
  public async Task Authenticated_external_identity_is_replaced_with_canonical_claims_before_authorization()
  {
    await using var app = await StartAsync(
      seedAuthenticatedExternalIdentity: true,
      includeTrustedFeature: true,
      acceptedIssuer: "https://idp-a.example");

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/protected",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var payload = await response.Content
      .ReadFromJsonAsync<ClaimsResponse>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(payload);
    Assert.Equal(
      "provider-a:tenant-a:external-subject",
      payload.Subject);
    Assert.Equal(
      "provider-a",
      payload.IdentityProvider);
    Assert.DoesNotContain(
      "role",
      payload.ClaimTypes);
    Assert.DoesNotContain(
      "scope",
      payload.ClaimTypes);
    Assert.Contains(
      SmartCitiesClaimTypes.Permission,
      payload.ClaimTypes);
  }

  [Fact]
  public async Task Authenticated_principal_without_trusted_external_identity_feature_fails_closed()
  {
    await using var app = await StartAsync(
      seedAuthenticatedExternalIdentity: true,
      includeTrustedFeature: false,
      acceptedIssuer: "https://idp-a.example");

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/protected",
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.Unauthorized,
      response.StatusCode);
  }

  [Fact]
  public async Task Unknown_provider_context_fails_closed_before_authorization()
  {
    await using var app = await StartAsync(
      seedAuthenticatedExternalIdentity: true,
      includeTrustedFeature: true,
      acceptedIssuer: "https://different-idp.example");

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/protected",
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.Unauthorized,
      response.StatusCode);
  }

  [Fact]
  public async Task Anonymous_requests_continue_to_public_endpoints_without_canonicalization()
  {
    await using var app = await StartAsync(
      seedAuthenticatedExternalIdentity: false,
      includeTrustedFeature: false,
      acceptedIssuer: "https://idp-a.example");

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/public",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  private static async Task<WebApplication> StartAsync(
    bool seedAuthenticatedExternalIdentity,
    bool includeTrustedFeature,
    string acceptedIssuer)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();

    builder.Services.AddSmartCitiesAuthenticationCanonicalization();
    builder.Services.AddSmartCitiesAuthorization();
    builder.Services.AddSingleton<
      IAuthenticationProviderAdapter>(
        new TestProviderAdapter(
          "provider-a",
          acceptedIssuer));

    var app = builder.Build();

    app.UseAuthentication();

    app.Use(
      async (context, next) =>
      {
        if (seedAuthenticatedExternalIdentity)
        {
          context.User = new ClaimsPrincipal(
            new ClaimsIdentity(
              [
                new Claim(
                  "sub",
                  "external-subject"),
                new Claim(
                  "role",
                  "provider-reviewer"),
                new Claim(
                  "scope",
                  "review.finalize"),
              ],
              authenticationType: "Bearer"));

          if (includeTrustedFeature)
          {
            context.SetValidatedExternalAuthentication(
              AuthenticationProviderContext.Create(
                authenticationScheme: "Bearer",
                issuer: "https://idp-a.example",
                tenantId: "tenant-a"),
              ExternalAuthenticatedIdentity.Create(
                subject: "external-subject",
                claims:
                [
                  ExternalIdentityClaim.Create(
                    "role",
                    "provider-reviewer"),
                  ExternalIdentityClaim.Create(
                    "scope",
                    "review.finalize"),
                ]));
          }
        }

        await next(context);
      });

    app.UseSmartCitiesAuthenticationCanonicalization();
    app.UseAuthorization();

    app.MapGet(
      "/public",
      static () => Results.Ok());

    app.MapGet(
        "/protected",
        static (ClaimsPrincipal user) =>
          Results.Ok(
            new ClaimsResponse(
              user.FindFirst(
                SmartCitiesClaimTypes.Subject)
                ?.Value,
              user.FindFirst(
                SmartCitiesClaimTypes.IdentityProvider)
                ?.Value,
              user.Claims
                .Select(static claim => claim.Type)
                .ToArray())))
      .RequireAuthorization(
        SmartCitiesPolicies.FinalizeDecisionReview);

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    return app;
  }

  private sealed record ClaimsResponse(
    string? Subject,
    string? IdentityProvider,
    IReadOnlyList<string> ClaimTypes);

  private sealed class TestProviderAdapter
    : IAuthenticationProviderAdapter
  {
    private readonly string acceptedIssuer;

    public TestProviderAdapter(
      string providerId,
      string acceptedIssuer)
    {
      ProviderId = providerId;
      this.acceptedIssuer = acceptedIssuer;
    }

    public string ProviderId { get; }

    public bool CanHandle(
      AuthenticationProviderContext context) =>
      string.Equals(
        acceptedIssuer,
        context.Issuer,
        StringComparison.Ordinal);

    public Task<CanonicalIdentity> NormalizeAsync(
      AuthenticationProviderContext context,
      ExternalAuthenticatedIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        CanonicalIdentity.Create(
          ProviderId,
          $"{ProviderId}:{context.TenantId}:{identity.Subject}",
          ["mobility-reviewer"],
          [SmartCitiesPermissions.FinalizeDecisionReview]));
    }
  }
}

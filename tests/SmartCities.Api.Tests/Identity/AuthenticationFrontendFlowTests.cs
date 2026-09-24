using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Identity;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Identity;

public sealed class AuthenticationFrontendFlowTests
{
  [Fact]
  public async Task Discovery_exposes_exactly_the_enabled_frontend_login_choices_without_secrets()
  {
    await using var app = await StartAsync(
      includeOidc: true);

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/api/authentication/providers",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var payload = await response.Content
      .ReadFromJsonAsync<
        IReadOnlyList<AuthenticationProviderDiscoveryResponse>>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(payload);
    Assert.Equal(2, payload.Count);

    var local = Assert.Single(
      payload,
      item => item.ProviderId == "local");
    Assert.Equal("credentials", local.LoginMode);
    Assert.Equal(
      "/api/authentication/local/session",
      local.SessionPath);
    Assert.Equal(
      "/api/authentication/local/token",
      local.TokenPath);
    Assert.Null(local.ChallengePath);

    var oidc = Assert.Single(
      payload,
      item => item.ProviderId == "google");
    Assert.Equal("redirect", oidc.LoginMode);
    Assert.Equal(
      "/api/authentication/providers/google/challenge",
      oidc.ChallengePath);
    Assert.Null(oidc.SessionPath);
    Assert.Null(oidc.TokenPath);

    var json = await response.Content.ReadAsStringAsync(
      TestContext.Current.CancellationToken);
    Assert.DoesNotContain(
      "oidc-secret",
      json,
      StringComparison.Ordinal);
    Assert.DoesNotContain(
      "0123456789abcdef0123456789abcdef",
      json,
      StringComparison.Ordinal);
  }

  [Fact]
  public void Challenge_uses_only_the_enabled_provider_scheme_and_safe_local_return_url()
  {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSmartCitiesAuthenticationProviders(
      BuildConfiguration(
        includeOidc: true));

    using var provider = services.BuildServiceProvider();
    var registry = provider.GetRequiredService<
      SmartCitiesAuthenticationProviderRegistry>();
    var controller =
      new AuthenticationProvidersController(registry);

    var result = controller.ChallengeProvider(
      "google",
      "/citizen/case/42");

    var challenge = Assert.IsType<ChallengeResult>(
      result);
    Assert.Equal(
      [SmartCitiesAuthenticationSchemes.Oidc("google")],
      challenge.AuthenticationSchemes);
    Assert.Equal(
      "/citizen/case/42",
      challenge.Properties?.RedirectUri);

    Assert.IsType<BadRequestObjectResult>(
      controller.ChallengeProvider(
        "google",
        "https://evil.example/phish"));

    Assert.IsType<NotFoundResult>(
      controller.ChallengeProvider(
        "missing",
        "/"));
  }

  [Fact]
  public async Task Local_session_credentials_issue_a_cookie_that_authorizes_the_next_request()
  {
    await using var app = await StartAsync();

    using var client = app.GetTestClient();
    using var login = await client.PostAsJsonAsync(
      "/api/authentication/local/session",
      new LocalCredentialRequest(
        "alice",
        "correct-password"),
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, login.StatusCode);

    var setCookie = Assert.Single(
      login.Headers.GetValues("Set-Cookie"));
    var cookie = setCookie.Split(';', 2)[0];

    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      "/protected");
    request.Headers.Add(
      "Cookie",
      cookie);

    using var protectedResponse = await client.SendAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.OK,
      protectedResponse.StatusCode);

    var principal = await protectedResponse.Content
      .ReadFromJsonAsync<ProtectedIdentityResponse>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(principal);
    Assert.Equal(
      "local:default:alice-001",
      principal.Subject);
    Assert.Equal("local", principal.IdentityProvider);
  }

  [Fact]
  public async Task Local_credentials_issue_a_bearer_token_that_round_trips_through_JWT_validation_and_canonicalization()
  {
    await using var app = await StartAsync();

    using var client = app.GetTestClient();
    using var tokenResponse = await client.PostAsJsonAsync(
      "/api/authentication/local/token",
      new LocalCredentialRequest(
        "alice",
        "correct-password"),
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.OK,
      tokenResponse.StatusCode);

    var token = await tokenResponse.Content
      .ReadFromJsonAsync<LocalAccessTokenResponse>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(token);
    Assert.Equal("Bearer", token.TokenType);
    Assert.False(
      string.IsNullOrWhiteSpace(
        token.AccessToken));
    Assert.InRange(
      token.ExpiresInSeconds,
      60,
      86400);

    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      "/protected");
    request.Headers.Authorization =
      new AuthenticationHeaderValue(
        "Bearer",
        token.AccessToken);

    using var protectedResponse = await client.SendAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.OK,
      protectedResponse.StatusCode);

    var principal = await protectedResponse.Content
      .ReadFromJsonAsync<ProtectedIdentityResponse>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(principal);
    Assert.Equal(
      "local:default:alice-001",
      principal.Subject);
    Assert.Equal("local", principal.IdentityProvider);
  }

  [Fact]
  public async Task Invalid_local_credentials_return_401_and_issue_neither_cookie_nor_token()
  {
    await using var app = await StartAsync();

    using var client = app.GetTestClient();

    using var session = await client.PostAsJsonAsync(
      "/api/authentication/local/session",
      new LocalCredentialRequest(
        "alice",
        "wrong-password"),
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.Unauthorized,
      session.StatusCode);
    Assert.False(
      session.Headers.Contains("Set-Cookie"));

    using var token = await client.PostAsJsonAsync(
      "/api/authentication/local/token",
      new LocalCredentialRequest(
        "alice",
        "wrong-password"),
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.Unauthorized,
      token.StatusCode);
  }

  private static async Task<WebApplication> StartAsync(
    bool includeOidc = false)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();

    builder.Services
      .AddSmartCitiesApiControllers()
      .AddApplicationPart(
        typeof(AuthenticationProvidersController).Assembly);
    builder.Services.AddSingleton<
      ILocalCredentialAuthenticator,
      TestLocalCredentialAuthenticator>();
    builder.Services.AddSmartCitiesAuthenticationProviders(
      BuildConfiguration(includeOidc));
    builder.Services.AddSmartCitiesAuthorization();

    var app = builder.Build();

    app.UseAuthentication();
    app.UseSmartCitiesAuthenticationCanonicalization();
    app.UseAuthorization();

    app.MapControllers();

    app.MapGet(
        "/protected",
        static (System.Security.Claims.ClaimsPrincipal user) =>
          Results.Ok(
            new ProtectedIdentityResponse(
              user.FindFirst(
                SmartCitiesClaimTypes.Subject)
                ?.Value,
              user.FindFirst(
                SmartCitiesClaimTypes.IdentityProvider)
                ?.Value)))
      .RequireAuthorization(
        SmartCitiesPolicies.FinalizeDecisionReview);

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    return app;
  }

  private static IConfiguration BuildConfiguration(
    bool includeOidc) =>
    new ConfigurationBuilder()
      .AddInMemoryCollection(
        BuildValues(includeOidc))
      .Build();

  private static Dictionary<string, string?> BuildValues(
    bool includeOidc)
  {
    var values =
      new Dictionary<string, string?>(
        StringComparer.Ordinal)
      {
        ["SmartCities:Authentication:Local:Enabled"] = "true",
        ["SmartCities:Authentication:Local:Issuer"] =
          "https://local.smartcities.test",
        ["SmartCities:Authentication:Local:Audience"] =
          "smartcities-api",
        ["SmartCities:Authentication:Local:SigningKey"] =
          "0123456789abcdef0123456789abcdef",
        ["SmartCities:Authentication:Local:CookieName"] =
          "smartcities.test",
        ["SmartCities:Authentication:Local:JwtLifetimeMinutes"] =
          "30",
        ["SmartCities:Authentication:Local:AllowedAuthorityRoles:0"] =
          "mobility-reviewer",
        ["SmartCities:Authentication:Local:AllowedPermissions:0"] =
          SmartCitiesPermissions.FinalizeDecisionReview,
      };

    if (includeOidc)
    {
      values[
        "SmartCities:Authentication:OpenIdConnect:google:Enabled"] =
        "true";
      values[
        "SmartCities:Authentication:OpenIdConnect:google:DisplayName"] =
        "Google";
      values[
        "SmartCities:Authentication:OpenIdConnect:google:Authority"] =
        "https://accounts.example.test";
      values[
        "SmartCities:Authentication:OpenIdConnect:google:ClientId"] =
        "oidc-client";
      values[
        "SmartCities:Authentication:OpenIdConnect:google:ClientSecret"] =
        "oidc-secret";
      values[
        "SmartCities:Authentication:OpenIdConnect:google:CallbackPath"] =
        "/signin-oidc-google";
    }

    return values;
  }

  private sealed class TestLocalCredentialAuthenticator
    : ILocalCredentialAuthenticator
  {
    public Task<ExternalAuthenticatedIdentity?> AuthenticateAsync(
      string userName,
      string password,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!string.Equals(
          userName,
          "alice",
          StringComparison.Ordinal)
        || !string.Equals(
          password,
          "correct-password",
          StringComparison.Ordinal))
      {
        return Task.FromResult<
          ExternalAuthenticatedIdentity?>(
            null);
      }

      return Task.FromResult<
        ExternalAuthenticatedIdentity?>(
          ExternalAuthenticatedIdentity.Create(
            "alice-001",
            [
              ExternalIdentityClaim.Create(
                "role",
                "mobility-reviewer"),
              ExternalIdentityClaim.Create(
                "permission",
                SmartCitiesPermissions
                  .FinalizeDecisionReview),
            ]));
    }
  }

  public sealed record ProtectedIdentityResponse(
    string? Subject,
    string? IdentityProvider);
}

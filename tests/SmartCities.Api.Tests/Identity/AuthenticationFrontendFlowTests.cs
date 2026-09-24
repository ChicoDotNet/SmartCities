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
using SmartCities.Api.Hosting;
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


  [Fact]
  public async Task Current_session_returns_anonymous_200_without_identity_material()
  {
    await using var app = await StartAsync();

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/api/authentication/session",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(
      response.Headers.CacheControl?.NoStore);

    var session = await response.Content
      .ReadFromJsonAsync<CurrentSessionSnapshot>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(session);
    Assert.False(session.Authenticated);
    Assert.Null(session.SubjectId);
    Assert.Null(session.IdentityProvider);
    Assert.Empty(session.AuthorityRoles);
    Assert.Empty(session.Permissions);
  }

  [Fact]
  public async Task Current_session_recovers_the_canonical_cookie_and_exposes_only_canonical_identity()
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
      "/api/authentication/session");
    request.Headers.Add("Cookie", cookie);

    using var response = await client.SendAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(
      response.Headers.CacheControl?.NoStore);

    var session = await response.Content
      .ReadFromJsonAsync<CurrentSessionSnapshot>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(session);
    Assert.True(session.Authenticated);
    Assert.Equal(
      "local:default:alice-001",
      session.SubjectId);
    Assert.Equal("local", session.IdentityProvider);
    Assert.Equal(
      ["mobility-reviewer"],
      session.AuthorityRoles);
    Assert.Equal(
      [SmartCitiesPermissions.FinalizeDecisionReview],
      session.Permissions);

    var json = await response.Content.ReadAsStringAsync(
      TestContext.Current.CancellationToken);
    Assert.DoesNotContain(
      "\"sub\"",
      json,
      StringComparison.Ordinal);
    Assert.DoesNotContain(
      "\"scope\"",
      json,
      StringComparison.Ordinal);
    Assert.DoesNotContain(
      "\"groups\"",
      json,
      StringComparison.Ordinal);
  }

  [Fact]
  public async Task Logout_requires_browser_request_marker_and_expires_the_shared_session_cookie()
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

    using var unmarked = new HttpRequestMessage(
      HttpMethod.Post,
      "/api/authentication/session/logout");
    unmarked.Headers.Add("Cookie", cookie);

    using var rejected = await client.SendAsync(
      unmarked,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.Forbidden,
      rejected.StatusCode);

    using var logout = new HttpRequestMessage(
      HttpMethod.Post,
      "/api/authentication/session/logout");
    logout.Headers.Add("Cookie", cookie);
    logout.Headers.Add(
      "X-SmartCities-Request",
      "browser");

    using var response = await client.SendAsync(
      logout,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.NoContent,
      response.StatusCode);
    Assert.True(
      response.Headers.CacheControl?.NoStore);

    var expiredCookie = Assert.Single(
      response.Headers.GetValues("Set-Cookie"));
    Assert.Contains(
      "smartcities.test=",
      expiredCookie,
      StringComparison.Ordinal);
    Assert.Contains(
      "expires=",
      expiredCookie,
      StringComparison.OrdinalIgnoreCase);
  }


  [Fact]
  public async Task Session_lifecycle_remains_safe_and_idempotent_when_no_provider_is_enabled()
  {
    await using var app = await StartAsync(
      includeLocal: false);

    using var client = app.GetTestClient();

    for (var attempt = 0; attempt < 2; attempt++)
    {
      using var logout = new HttpRequestMessage(
        HttpMethod.Post,
        "/api/authentication/session/logout");
      logout.Headers.Add(
        "X-SmartCities-Request",
        "browser");

      using var logoutResponse = await client.SendAsync(
        logout,
        TestContext.Current.CancellationToken);

      Assert.Equal(
        HttpStatusCode.NoContent,
        logoutResponse.StatusCode);
      Assert.True(
        logoutResponse.Headers.CacheControl?.NoStore);
    }

    using var current = await client.GetAsync(
      "/api/authentication/session",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, current.StatusCode);

    var session = await current.Content
      .ReadFromJsonAsync<CurrentSessionSnapshot>(
        cancellationToken:
          TestContext.Current.CancellationToken);

    Assert.NotNull(session);
    Assert.False(session.Authenticated);
  }

  private static async Task<WebApplication> StartAsync(
    bool includeOidc = false,
    bool includeLocal = true)
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
      BuildConfiguration(
        includeOidc,
        includeLocal));
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
    bool includeOidc,
    bool includeLocal = true) =>
    new ConfigurationBuilder()
      .AddInMemoryCollection(
        BuildValues(
          includeOidc,
          includeLocal))
      .Build();

  private static Dictionary<string, string?> BuildValues(
    bool includeOidc,
    bool includeLocal = true)
  {
    var values =
      new Dictionary<string, string?>(
        StringComparer.Ordinal);

    if (includeLocal)
    {
      values["SmartCities:Authentication:Local:Enabled"] =
        "true";
      values["SmartCities:Authentication:Local:Issuer"] =
        "https://local.smartcities.test";
      values["SmartCities:Authentication:Local:Audience"] =
        "smartcities-api";
      values["SmartCities:Authentication:Local:SigningKey"] =
        "0123456789abcdef0123456789abcdef";
      values["SmartCities:Authentication:Local:CookieName"] =
        "smartcities.test";
      values["SmartCities:Authentication:Local:JwtLifetimeMinutes"] =
        "30";
      values[
        "SmartCities:Authentication:Local:AllowedAuthorityRoles:0"] =
        "mobility-reviewer";
      values[
        "SmartCities:Authentication:Local:AllowedPermissions:0"] =
        SmartCitiesPermissions.FinalizeDecisionReview;
    }

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


  private sealed record CurrentSessionSnapshot(
    bool Authenticated,
    string? SubjectId,
    string? IdentityProvider,
    IReadOnlyList<string> AuthorityRoles,
    IReadOnlyList<string> Permissions);

  public sealed record ProtectedIdentityResponse(
    string? Subject,
    string? IdentityProvider);
}

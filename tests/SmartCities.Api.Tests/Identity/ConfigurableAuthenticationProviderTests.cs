using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartCities.Api.Identity;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Identity;

public sealed class ConfigurableAuthenticationProviderTests
{
  [Fact]
  public async Task Absent_provider_configuration_keeps_all_real_providers_off()
  {
    var services = new ServiceCollection();
    services.AddLogging();

    services.AddSmartCitiesAuthenticationProviders(
      new ConfigurationBuilder().Build());

    using var provider = services.BuildServiceProvider();
    var registry = provider.GetRequiredService<
      SmartCitiesAuthenticationProviderRegistry>();
    var schemes = provider.GetRequiredService<
      IAuthenticationSchemeProvider>();

    Assert.Empty(registry.Providers);

    var registered = await schemes.GetAllSchemesAsync();

    Assert.DoesNotContain(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.Session);
    Assert.DoesNotContain(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.LocalJwt);
  }

  [Fact]
  public async Task Complete_local_configuration_registers_cookie_and_jwt_in_parallel()
  {
    var services = new ServiceCollection();
    services.AddLogging();

    services.AddSmartCitiesAuthenticationProviders(
      BuildConfiguration(
        new Dictionary<string, string?>
        {
          ["SmartCities:Authentication:Local:Enabled"] = "true",
          ["SmartCities:Authentication:Local:Issuer"] = "https://local.smartcities.test",
          ["SmartCities:Authentication:Local:Audience"] = "smartcities-api",
          ["SmartCities:Authentication:Local:SigningKey"] = "0123456789abcdef0123456789abcdef",
          ["SmartCities:Authentication:Local:CookieName"] = "smartcities.test",
          ["SmartCities:Authentication:Local:AllowedAuthorityRoles:0"] = "citizen",
          ["SmartCities:Authentication:Local:AllowedAuthorityRoles:1"] = "mobility-reviewer",
          ["SmartCities:Authentication:Local:AllowedPermissions:0"] = SmartCitiesPermissions.FinalizeDecisionReview,
        }));

    using var provider = services.BuildServiceProvider();
    var registry = provider.GetRequiredService<
      SmartCitiesAuthenticationProviderRegistry>();
    var descriptor = Assert.Single(
      registry.Providers);

    Assert.Equal("local", descriptor.ProviderId);
    Assert.Equal(
      SmartCitiesAuthenticationProviderKind.Local,
      descriptor.Kind);

    var schemes = provider.GetRequiredService<
      IAuthenticationSchemeProvider>();
    var registered = await schemes.GetAllSchemesAsync();

    Assert.Contains(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.Session);
    Assert.Contains(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.LocalJwt);

    var cookies = provider.GetRequiredService<
      IOptionsMonitor<CookieAuthenticationOptions>>()
      .Get(SmartCitiesAuthenticationSchemes.Session);

    Assert.Equal(
      "smartcities.test",
      cookies.Cookie.Name);

    var jwt = provider.GetRequiredService<
      IOptionsMonitor<JwtBearerOptions>>()
      .Get(SmartCitiesAuthenticationSchemes.LocalJwt);

    Assert.Equal(
      "https://local.smartcities.test",
      jwt.TokenValidationParameters.ValidIssuer);
    Assert.Equal(
      "smartcities-api",
      jwt.TokenValidationParameters.ValidAudience);
  }

  [Fact]
  public void Partial_local_configuration_fails_fast()
  {
    var services = new ServiceCollection();
    services.AddLogging();

    var exception = Assert.Throws<InvalidOperationException>(
      () => services.AddSmartCitiesAuthenticationProviders(
        BuildConfiguration(
          new Dictionary<string, string?>
          {
            ["SmartCities:Authentication:Local:Issuer"] =
              "https://local.smartcities.test",
          })));

    Assert.Contains(
      "Local",
      exception.Message,
      StringComparison.Ordinal);
  }

  [Fact]
  public async Task Multiple_complete_oidc_providers_are_registered_together()
  {
    var services = new ServiceCollection();
    services.AddLogging();

    services.AddSmartCitiesAuthenticationProviders(
      BuildConfiguration(
        OidcConfiguration()));

    using var provider = services.BuildServiceProvider();
    var registry = provider.GetRequiredService<
      SmartCitiesAuthenticationProviderRegistry>();

    Assert.Equal(
      ["google", "workforce"],
      registry.Providers
        .Select(static item => item.ProviderId)
        .Order(StringComparer.Ordinal)
        .ToArray());

    Assert.All(
      registry.Providers,
      item => Assert.Equal(
        SmartCitiesAuthenticationProviderKind.OpenIdConnect,
        item.Kind));

    var schemes = provider.GetRequiredService<
      IAuthenticationSchemeProvider>();
    var registered = await schemes.GetAllSchemesAsync();

    Assert.Contains(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.Session);
    Assert.Contains(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.Oidc("google"));
    Assert.Contains(
      registered,
      item => item.Name == SmartCitiesAuthenticationSchemes.Oidc("workforce"));

    var oidc = provider.GetRequiredService<
      IOptionsMonitor<OpenIdConnectOptions>>();

    Assert.Equal(
      "https://accounts.example.test",
      oidc.Get(
        SmartCitiesAuthenticationSchemes.Oidc("google"))
        .Authority);
    Assert.Equal(
      SmartCitiesAuthenticationSchemes.Session,
      oidc.Get(
        SmartCitiesAuthenticationSchemes.Oidc("google"))
        .SignInScheme);
  }

  [Fact]
  public void Explicitly_disabled_oidc_provider_is_not_registered_or_validated()
  {
    var values = OidcConfiguration();
    values["SmartCities:Authentication:OpenIdConnect:google:Enabled"] = "false";
    values.Remove("SmartCities:Authentication:OpenIdConnect:google:ClientSecret");

    var services = new ServiceCollection();
    services.AddLogging();

    services.AddSmartCitiesAuthenticationProviders(
      BuildConfiguration(values));

    using var provider = services.BuildServiceProvider();
    var registry = provider.GetRequiredService<
      SmartCitiesAuthenticationProviderRegistry>();

    Assert.DoesNotContain(
      registry.Providers,
      item => item.ProviderId == "google");
    Assert.Contains(
      registry.Providers,
      item => item.ProviderId == "workforce");
  }

  [Fact]
  public void Partial_oidc_provider_configuration_fails_fast()
  {
    var services = new ServiceCollection();
    services.AddLogging();

    var exception = Assert.Throws<InvalidOperationException>(
      () => services.AddSmartCitiesAuthenticationProviders(
        BuildConfiguration(
          new Dictionary<string, string?>
          {
            ["SmartCities:Authentication:OpenIdConnect:google:Authority"] =
              "https://accounts.example.test",
            ["SmartCities:Authentication:OpenIdConnect:google:ClientId"] =
              "client-google",
          })));

    Assert.Contains(
      "google",
      exception.Message,
      StringComparison.Ordinal);
  }

  [Fact]
  public async Task Generic_oidc_adapter_maps_only_explicitly_allowed_claim_values()
  {
    var services = new ServiceCollection();
    services.AddLogging();

    var values = OidcConfiguration();
    values["SmartCities:Authentication:OpenIdConnect:workforce:DefaultAuthorityRoles:0"] =
      "citizen";
    values["SmartCities:Authentication:OpenIdConnect:workforce:RoleClaimType"] =
      "groups";
    values["SmartCities:Authentication:OpenIdConnect:workforce:RoleMappings:external-reviewers"] =
      "mobility-reviewer";
    values["SmartCities:Authentication:OpenIdConnect:workforce:PermissionClaimType"] =
      "scope";
    values["SmartCities:Authentication:OpenIdConnect:workforce:PermissionMappings:review.finalize"] =
      SmartCitiesPermissions.FinalizeDecisionReview;

    services.AddSmartCitiesAuthenticationProviders(
      BuildConfiguration(values));

    using var provider = services.BuildServiceProvider();
    var adapter = provider
      .GetServices<IAuthenticationProviderAdapter>()
      .Single(
        item => item.ProviderId == "workforce");

    var context = AuthenticationProviderContext.Create(
      SmartCitiesAuthenticationSchemes.Oidc("workforce"),
      "https://workforce.example.test",
      "tenant-a");
    var external = ExternalAuthenticatedIdentity.Create(
      "external-user-42",
      [
        ExternalIdentityClaim.Create(
          "groups",
          "external-reviewers"),
        ExternalIdentityClaim.Create(
          "groups",
          "unmapped-admins"),
        ExternalIdentityClaim.Create(
          "scope",
          "review.finalize"),
        ExternalIdentityClaim.Create(
          "scope",
          "unmapped.superuser"),
      ]);

    var canonical = await adapter.NormalizeAsync(
      context,
      external,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      "workforce",
      canonical.IdentityProvider);
    Assert.Equal(
      "workforce:tenant-a:external-user-42",
      canonical.SubjectId);
    Assert.Equal(
      ["citizen", "mobility-reviewer"],
      canonical.AuthorityRoles);
    Assert.Equal(
      [SmartCitiesPermissions.FinalizeDecisionReview],
      canonical.Permissions);
  }

  private static IConfiguration BuildConfiguration(
    IDictionary<string, string?> values) =>
    new ConfigurationBuilder()
      .AddInMemoryCollection(values)
      .Build();

  private static Dictionary<string, string?> OidcConfiguration() =>
    new(StringComparer.Ordinal)
    {
      ["SmartCities:Authentication:OpenIdConnect:google:Enabled"] = "true",
      ["SmartCities:Authentication:OpenIdConnect:google:DisplayName"] = "Google",
      ["SmartCities:Authentication:OpenIdConnect:google:Authority"] =
        "https://accounts.example.test",
      ["SmartCities:Authentication:OpenIdConnect:google:ClientId"] =
        "client-google",
      ["SmartCities:Authentication:OpenIdConnect:google:ClientSecret"] =
        "secret-google",
      ["SmartCities:Authentication:OpenIdConnect:google:CallbackPath"] =
        "/signin-oidc-google",
      ["SmartCities:Authentication:OpenIdConnect:workforce:Enabled"] = "true",
      ["SmartCities:Authentication:OpenIdConnect:workforce:DisplayName"] =
        "Workforce",
      ["SmartCities:Authentication:OpenIdConnect:workforce:Authority"] =
        "https://workforce.example.test",
      ["SmartCities:Authentication:OpenIdConnect:workforce:ClientId"] =
        "client-workforce",
      ["SmartCities:Authentication:OpenIdConnect:workforce:ClientSecret"] =
        "secret-workforce",
      ["SmartCities:Authentication:OpenIdConnect:workforce:CallbackPath"] =
        "/signin-oidc-workforce",
    };
}

using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Composes independently configurable local and OpenID Connect authentication providers.
/// </summary>
public static class SmartCitiesAuthenticationProviderExtensions
{
  /// <summary>
  /// Registers every enabled authentication provider and leaves absent/disabled providers unregistered.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <param name="configuration">Host configuration containing optional provider sections.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  public static IServiceCollection AddSmartCitiesAuthenticationProviders(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddSmartCitiesAuthenticationCanonicalization();

    var configured =
      SmartCitiesAuthenticationProviderConfiguration.Read(
        configuration);
    var descriptors =
      new List<SmartCitiesAuthenticationProviderDescriptor>();

    var authentication = services
      .AddAuthentication(
        options =>
        {
          options.DefaultAuthenticateScheme =
            SmartCitiesAuthenticationSchemes.Router;
          options.DefaultChallengeScheme =
            SmartCitiesNoProviderAuthenticationHandler.SchemeName;
        })
      .AddPolicyScheme(
        SmartCitiesAuthenticationSchemes.Router,
        displayName: null,
        options =>
        {
          options.ForwardDefaultSelector =
            context =>
            {
              var routing = context.RequestServices
                .GetRequiredService<
                  SmartCitiesAuthenticationRouting>();

              var authorization =
                context.Request.Headers.Authorization.ToString();

              if (routing.LocalJwtEnabled
                && authorization.StartsWith(
                  "Bearer ",
                  StringComparison.OrdinalIgnoreCase))
              {
                return SmartCitiesAuthenticationSchemes.LocalJwt;
              }

              return routing.SessionEnabled
                ? SmartCitiesAuthenticationSchemes.Session
                : SmartCitiesNoProviderAuthenticationHandler.SchemeName;
            };
        });

    if (configured.RequiresSession)
    {
      authentication.AddCookie(
        SmartCitiesAuthenticationSchemes.Session,
        options =>
        {
          options.Cookie.Name =
            configured.Local?.CookieName
            ?? "smartcities.session";
          options.Cookie.HttpOnly = true;
          options.Cookie.SameSite =
            SameSiteMode.Lax;
          options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;
          options.SlidingExpiration = true;
          options.ExpireTimeSpan =
            TimeSpan.FromHours(8);
          options.Events =
            new CookieAuthenticationEvents
            {
              OnValidatePrincipal =
                ValidateCanonicalSessionAsync,
            };
        });
    }

    if (configured.Local is not null)
    {
      RegisterLocal(
        services,
        authentication,
        configured.Local);

      descriptors.Add(
        new SmartCitiesAuthenticationProviderDescriptor(
          "local",
          "Local",
          SmartCitiesAuthenticationProviderKind.Local,
          [
            SmartCitiesAuthenticationSchemes.Session,
            SmartCitiesAuthenticationSchemes.LocalJwt,
          ],
          challengeScheme: null));
    }

    foreach (var oidc in configured.OpenIdConnect)
    {
      RegisterOidc(
        services,
        authentication,
        oidc);

      descriptors.Add(
        new SmartCitiesAuthenticationProviderDescriptor(
          oidc.ProviderId,
          oidc.DisplayName,
          SmartCitiesAuthenticationProviderKind.OpenIdConnect,
          [
            oidc.SchemeName,
            SmartCitiesAuthenticationSchemes.Session,
          ],
          oidc.SchemeName));
    }

    services.Replace(
      ServiceDescriptor.Singleton(
        new SmartCitiesAuthenticationRouting(
          configured.RequiresSession,
          configured.Local is not null)));
    services.AddSingleton(
      new SmartCitiesAuthenticationProviderRegistry(
        descriptors));

    return services;
  }

  private static void RegisterLocal(
    IServiceCollection services,
    AuthenticationBuilder authentication,
    LocalAuthenticationProviderConfiguration configuration)
  {
    services.AddSingleton<IAuthenticationProviderAdapter>(
      new ConfiguredLocalAuthenticationProviderAdapter(
        configuration));

    authentication.AddJwtBearer(
      SmartCitiesAuthenticationSchemes.LocalJwt,
      options =>
      {
        options.MapInboundClaims = false;
        options.TokenValidationParameters =
          new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidIssuer = configuration.Issuer,
            ValidateAudience = true,
            ValidAudience = configuration.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
              new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                  configuration.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType =
              configuration.SubjectClaimType,
            RoleClaimType =
              configuration.RoleClaimType,
          };
        options.Events =
          new JwtBearerEvents
          {
            OnTokenValidated =
              context => CanonicalizeLocalJwtAsync(
                context,
                configuration),
          };
      });
  }

  private static void RegisterOidc(
    IServiceCollection services,
    AuthenticationBuilder authentication,
    OidcAuthenticationProviderConfiguration configuration)
  {
    services.AddSingleton<IAuthenticationProviderAdapter>(
      new ConfiguredOidcAuthenticationProviderAdapter(
        configuration));

    authentication.AddOpenIdConnect(
      configuration.SchemeName,
      configuration.DisplayName,
      options =>
      {
        options.Authority =
          configuration.Authority;
        options.ClientId =
          configuration.ClientId;
        options.ClientSecret =
          configuration.ClientSecret;
        options.CallbackPath =
          configuration.CallbackPath;
        options.SignInScheme =
          SmartCitiesAuthenticationSchemes.Session;
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = false;
        options.MapInboundClaims = false;
        options.Events =
          new OpenIdConnectEvents
          {
            OnTokenValidated =
              context => CanonicalizeOidcAsync(
                context,
                configuration),
          };
      });
  }

  private static async Task CanonicalizeLocalJwtAsync(
    Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext context,
    LocalAuthenticationProviderConfiguration configuration)
  {
    var principal = context.Principal
      ?? throw new InvalidOperationException(
        "Validated local JWT did not provide a principal.");
    var subject = RequiredClaim(
      principal,
      configuration.SubjectClaimType,
      "local JWT subject");
    var external = ExternalAuthenticatedIdentity.Create(
      subject,
      ToExternalClaims(principal));
    var providerContext =
      AuthenticationProviderContext.Create(
        SmartCitiesAuthenticationSchemes.LocalJwt,
        configuration.Issuer,
        tenantId: null);
    var canonicalizer = context.HttpContext.RequestServices
      .GetRequiredService<
        IAuthenticationCanonicalizer>();
    var canonical = await canonicalizer
      .CanonicalizeAsync(
        providerContext,
        external,
        context.HttpContext.RequestAborted)
      .ConfigureAwait(false);

    context.Principal =
      canonical.ToClaimsPrincipal(
        SmartCitiesAuthenticationSchemes.LocalJwt);
    context.HttpContext.SetValidatedCanonicalAuthentication(
      canonical,
      SmartCitiesAuthenticationSchemes.LocalJwt);
  }

  private static async Task CanonicalizeOidcAsync(
    Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context,
    OidcAuthenticationProviderConfiguration configuration)
  {
    var principal = context.Principal
      ?? throw new InvalidOperationException(
        $"Validated OIDC provider '{configuration.ProviderId}' did not provide a principal.");
    var subject = RequiredClaim(
      principal,
      configuration.SubjectClaimType,
      $"OIDC provider '{configuration.ProviderId}' subject");
    var tenantId =
      configuration.TenantClaimType is null
        ? null
        : principal.FindFirst(
            configuration.TenantClaimType)
          ?.Value;
    var external = ExternalAuthenticatedIdentity.Create(
      subject,
      ToExternalClaims(principal));
    var providerContext =
      AuthenticationProviderContext.Create(
        configuration.SchemeName,
        configuration.Authority,
        tenantId);
    var canonicalizer = context.HttpContext.RequestServices
      .GetRequiredService<
        IAuthenticationCanonicalizer>();
    var canonical = await canonicalizer
      .CanonicalizeAsync(
        providerContext,
        external,
        context.HttpContext.RequestAborted)
      .ConfigureAwait(false);

    context.Principal =
      canonical.ToClaimsPrincipal(
        configuration.SchemeName);
    context.HttpContext.SetValidatedCanonicalAuthentication(
      canonical,
      configuration.SchemeName);
  }

  private static Task ValidateCanonicalSessionAsync(
    CookieValidatePrincipalContext context)
  {
    try
    {
      var principal = context.Principal
        ?? throw new InvalidOperationException(
          "Session cookie did not contain a principal.");
      var canonical =
        principal.ToCanonicalIdentity();

      context.HttpContext.SetValidatedCanonicalAuthentication(
        canonical,
        SmartCitiesAuthenticationSchemes.Session);

      return Task.CompletedTask;
    }
    catch (InvalidOperationException)
    {
      context.RejectPrincipal();
      return context.HttpContext.SignOutAsync(
        SmartCitiesAuthenticationSchemes.Session);
    }
  }

  private static string RequiredClaim(
    ClaimsPrincipal principal,
    string claimType,
    string description)
  {
    var values = principal.FindAll(claimType)
      .Select(static claim => claim.Value)
      .Where(
        static value =>
          !string.IsNullOrWhiteSpace(value))
      .Distinct(StringComparer.Ordinal)
      .ToArray();

    if (values.Length != 1)
    {
      throw new InvalidOperationException(
        $"The validated principal must contain exactly one {description} claim.");
    }

    return values[0];
  }

  private static ExternalIdentityClaim[] ToExternalClaims(
    ClaimsPrincipal principal) =>
    principal.Claims
      .Where(
        static claim =>
          !string.IsNullOrWhiteSpace(claim.Type)
          && !string.IsNullOrWhiteSpace(claim.Value))
      .Select(
        static claim =>
          ExternalIdentityClaim.Create(
            claim.Type,
            claim.Value))
      .ToArray();
}

internal sealed record SmartCitiesAuthenticationRouting(
  bool SessionEnabled,
  bool LocalJwtEnabled);

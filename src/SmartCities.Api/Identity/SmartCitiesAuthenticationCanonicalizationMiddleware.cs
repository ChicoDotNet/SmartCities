using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Replaces an authenticated provider-native request principal with the canonical SmartCities principal.
/// </summary>
public sealed class SmartCitiesAuthenticationCanonicalizationMiddleware
{
  private readonly RequestDelegate next;
  private readonly ILogger<SmartCitiesAuthenticationCanonicalizationMiddleware> logger;

  /// <summary>Initializes the canonicalization middleware.</summary>
  /// <param name="next">Next middleware in the request pipeline.</param>
  /// <param name="logger">Operational logger for fail-closed authentication-boundary events.</param>
  public SmartCitiesAuthenticationCanonicalizationMiddleware(
    RequestDelegate next,
    ILogger<SmartCitiesAuthenticationCanonicalizationMiddleware> logger)
  {
    ArgumentNullException.ThrowIfNull(next);
    ArgumentNullException.ThrowIfNull(logger);

    this.next = next;
    this.logger = logger;
  }

  /// <summary>Canonicalizes authenticated external identities before authorization.</summary>
  /// <param name="context">Current HTTP context.</param>
  /// <param name="canonicalizer">Request-scoped canonicalization orchestrator.</param>
  /// <returns>A task representing the request pipeline.</returns>
  public async Task InvokeAsync(
    HttpContext context,
    IAuthenticationCanonicalizer canonicalizer)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(canonicalizer);

    if (!context.User.Identities.Any(
        static identity => identity.IsAuthenticated))
    {
      await next(context).ConfigureAwait(false);
      return;
    }

    var feature = context.Features
      .Get<IValidatedExternalAuthenticationFeature>();

    if (feature is null)
    {
      AuthenticationCanonicalizationLog.MissingTrustedFeature(
        logger,
        context.User.Identity?.AuthenticationType
          ?? "unknown");

      context.Response.StatusCode =
        StatusCodes.Status401Unauthorized;
      return;
    }

    CanonicalIdentity canonicalIdentity;

    try
    {
      canonicalIdentity = await canonicalizer
        .CanonicalizeAsync(
          feature.ProviderContext,
          feature.ExternalIdentity,
          context.RequestAborted)
        .ConfigureAwait(false);
    }
    catch (AuthenticationCanonicalizationException exception)
    {
      AuthenticationCanonicalizationLog.Rejected(
        logger,
        feature.ProviderContext.AuthenticationScheme,
        exception);

      context.Response.StatusCode =
        StatusCodes.Status401Unauthorized;
      return;
    }

    context.User = canonicalIdentity.ToClaimsPrincipal(
      feature.ProviderContext.AuthenticationScheme);

    await next(context).ConfigureAwait(false);
  }
}

internal static partial class AuthenticationCanonicalizationLog
{
  [LoggerMessage(
    EventId = 1200,
    EventName = "MissingValidatedExternalAuthentication",
    Level = LogLevel.Warning,
    Message = "Authenticated request from scheme {AuthenticationScheme} reached canonicalization without validated external-authentication context.")]
  internal static partial void MissingTrustedFeature(
    ILogger logger,
    string authenticationScheme);

  [LoggerMessage(
    EventId = 1201,
    EventName = "AuthenticationCanonicalizationRejected",
    Level = LogLevel.Warning,
    Message = "Authenticated request from scheme {AuthenticationScheme} was rejected by the SmartCities canonicalization boundary.")]
  internal static partial void Rejected(
    ILogger logger,
    string authenticationScheme,
    Exception exception);
}

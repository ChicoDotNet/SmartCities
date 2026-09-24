using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Carries a canonical identity produced by a trusted SmartCities authentication handler.
/// </summary>
public interface IValidatedCanonicalAuthenticationFeature
{
  /// <summary>Gets the canonical provider-neutral identity.</summary>
  CanonicalIdentity Identity { get; }

  /// <summary>Gets the ASP.NET Core scheme that established the trusted canonical identity.</summary>
  string AuthenticationScheme { get; }
}

internal sealed class ValidatedCanonicalAuthenticationFeature
  : IValidatedCanonicalAuthenticationFeature
{
  internal ValidatedCanonicalAuthenticationFeature(
    CanonicalIdentity identity,
    string authenticationScheme)
  {
    ArgumentNullException.ThrowIfNull(identity);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      authenticationScheme);

    Identity = identity;
    AuthenticationScheme =
      authenticationScheme.Trim();
  }

  public CanonicalIdentity Identity { get; }

  public string AuthenticationScheme { get; }
}

/// <summary>
/// Stores trusted canonical identity handoff data on the current HTTP request.
/// </summary>
public static class ValidatedCanonicalAuthenticationHttpContextExtensions
{
  /// <summary>
  /// Stores a canonical identity produced by a trusted authentication handler for request-pipeline consumption.
  /// </summary>
  public static void SetValidatedCanonicalAuthentication(
    this HttpContext context,
    CanonicalIdentity identity,
    string authenticationScheme)
  {
    ArgumentNullException.ThrowIfNull(context);

    context.Features.Set<
      IValidatedCanonicalAuthenticationFeature>(
        new ValidatedCanonicalAuthenticationFeature(
          identity,
          authenticationScheme));
  }
}

using System.Collections.ObjectModel;

namespace SmartCities.Api.Identity;

/// <summary>
/// Describes one enabled authentication provider without exposing security secrets.
/// </summary>
public sealed record SmartCitiesAuthenticationProviderDescriptor
{
  internal SmartCitiesAuthenticationProviderDescriptor(
    string providerId,
    string displayName,
    SmartCitiesAuthenticationProviderKind kind,
    IEnumerable<string> authenticationSchemes,
    string? challengeScheme)
  {
    ProviderId = providerId;
    DisplayName = displayName;
    Kind = kind;
    AuthenticationSchemes =
      new ReadOnlyCollection<string>(
        authenticationSchemes.ToArray());
    ChallengeScheme = challengeScheme;
  }

  /// <summary>Gets the stable SmartCities provider identifier.</summary>
  public string ProviderId { get; }

  /// <summary>Gets the citizen/operator-facing provider display name.</summary>
  public string DisplayName { get; }

  /// <summary>Gets the provider composition pattern.</summary>
  public SmartCitiesAuthenticationProviderKind Kind { get; }

  /// <summary>Gets the ASP.NET Core schemes registered for this provider.</summary>
  public IReadOnlyList<string> AuthenticationSchemes { get; }

  /// <summary>Gets the optional interactive challenge scheme.</summary>
  public string? ChallengeScheme { get; }
}

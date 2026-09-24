namespace SmartCities.Api.Identity;

/// <summary>
/// Describes one frontend-safe enabled sign-in choice.
/// </summary>
public sealed record AuthenticationProviderDiscoveryResponse(
  string ProviderId,
  string DisplayName,
  string LoginMode,
  string? ChallengePath,
  string? SessionPath,
  string? TokenPath);

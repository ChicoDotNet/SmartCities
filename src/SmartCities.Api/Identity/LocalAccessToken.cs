namespace SmartCities.Api.Identity;

/// <summary>
/// Represents a locally issued bearer token and its relative validity period.
/// </summary>
public sealed record LocalAccessToken(
  string Value,
  int ExpiresInSeconds);

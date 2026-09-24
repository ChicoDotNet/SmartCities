namespace SmartCities.Api.Identity;

/// <summary>
/// Represents a successful local bearer-token issuance response.
/// </summary>
public sealed record LocalAccessTokenResponse(
  string AccessToken,
  string TokenType,
  int ExpiresInSeconds);

namespace SmartCities.Api.Identity;

/// <summary>
/// Defines stable ASP.NET Core authentication-scheme names used by SmartCities provider composition.
/// </summary>
public static class SmartCitiesAuthenticationSchemes
{
  /// <summary>Policy scheme that routes request authentication to an enabled concrete scheme.</summary>
  public const string Router =
    "SmartCities.Authentication";

  /// <summary>Shared encrypted browser-session cookie for canonical SmartCities identities.</summary>
  public const string Session =
    "SmartCities.Session";

  /// <summary>Local JWT bearer scheme.</summary>
  public const string LocalJwt =
    "SmartCities.Local.Jwt";

  /// <summary>Returns the unique challenge/callback scheme for a configured OIDC provider.</summary>
  public static string Oidc(string providerId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(providerId);

    return $"SmartCities.Oidc.{providerId.Trim()}";
  }
}

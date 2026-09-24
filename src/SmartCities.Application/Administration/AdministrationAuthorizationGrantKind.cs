namespace SmartCities.Application.Administration;

/// <summary>
/// Defines the canonical authorization material that can be assigned to an admitted Administration identity target.
/// </summary>
public enum AdministrationAuthorizationGrantKind
{
  /// <summary>Assigns one canonical SmartCities authority role.</summary>
  AuthorityRole = 0,

  /// <summary>Assigns one canonical SmartCities permission.</summary>
  Permission = 1,
}

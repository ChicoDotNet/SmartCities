namespace SmartCities.Identity;

/// <summary>
/// Defines stable SmartCities authority-role values independently of authentication providers.
/// </summary>
public static class SmartCitiesAuthorityRoles
{
  /// <summary>Town Hall administrative authority.</summary>
  public const string TownHallAdministrator =
    "town-hall-admin";

  /// <summary>Human reviewer responsible for citizen mobility decisions.</summary>
  public const string MobilityReviewer =
    "mobility-reviewer";

  /// <summary>Gets all code-owned authority roles assignable by Town Hall Administration.</summary>
  public static IReadOnlyList<string> All { get; } =
    Array.AsReadOnly(
      new[]
      {
        MobilityReviewer,
        TownHallAdministrator,
      });
}

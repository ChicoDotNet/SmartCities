namespace SmartCities.Application.FeatureFlags;

/// <summary>
/// Defines stable machine identifiers for independently deployable SmartCities vertical slices.
/// </summary>
public static class SmartCitiesFeatures
{
  /// <summary>The citizen mobility report → human-reviewed outcome vertical slice.</summary>
  public const string CitizenMobility =
    "citizen-mobility";

  /// <summary>Gets all registered vertical-slice feature identifiers.</summary>
  public static IReadOnlyList<string> All { get; } =
    Array.AsReadOnly(
      new[]
      {
        CitizenMobility,
      });
}

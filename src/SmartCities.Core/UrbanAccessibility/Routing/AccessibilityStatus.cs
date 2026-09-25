namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Represents normalized accessibility knowledge without inventing unsupported certainty.
/// </summary>
public enum AccessibilityStatus
{
  /// <summary>The source does not provide enough information to make an accessibility claim.</summary>
  Unknown = 0,

  /// <summary>The source explicitly indicates accessibility with no known normalized limitations.</summary>
  KnownAccessible = 1,

  /// <summary>The source exposes one or more known accessibility limitations.</summary>
  KnownLimited = 2,

  /// <summary>The source explicitly indicates that the leg/journey is not accessible for the represented context.</summary>
  KnownInaccessible = 3,
}

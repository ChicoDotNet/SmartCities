namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Defines provider-neutral expected routing failure categories.
/// </summary>
public enum AccessibilityRoutingFailureCode
{
  /// <summary>No feasible journey was found for the normalized request.</summary>
  NoRoute = 1,

  /// <summary>The configured engine does not support the requested capability or semantics.</summary>
  UnsupportedRequest = 2,

  /// <summary>Required source/network/schedule data is unavailable.</summary>
  SourceDataUnavailable = 3,

  /// <summary>Required source/network/schedule data is invalid or unusable.</summary>
  SourceDataInvalid = 4,

  /// <summary>The routing engine is not currently available.</summary>
  EngineUnavailable = 5,

  /// <summary>The routing operation exceeded the caller execution timeout.</summary>
  TimedOut = 6,

  /// <summary>The engine failed in a way not represented by a more specific expected category.</summary>
  EngineFailure = 7,
}

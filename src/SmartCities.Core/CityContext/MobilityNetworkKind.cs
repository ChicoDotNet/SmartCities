namespace SmartCities.CityContext;

/// <summary>
/// Identifies the minimal network families required by Urban Accessibility V1.
/// </summary>
public enum MobilityNetworkKind
{
  /// <summary>A pedestrian/walking network or network dataset.</summary>
  Pedestrian = 1,

  /// <summary>A scheduled public-transport network or network dataset.</summary>
  PublicTransport = 2,
}

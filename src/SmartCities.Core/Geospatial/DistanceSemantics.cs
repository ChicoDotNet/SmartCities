namespace SmartCities.Geospatial;

/// <summary>
/// Describes how a distance quantity was derived without coupling SmartCities to a calculation engine.
/// </summary>
public enum DistanceSemantics
{
  /// <summary>Shortest surface distance on an earth/geodetic model.</summary>
  Geodesic = 1,

  /// <summary>Straight-line distance measured in a projected/cartesian plane.</summary>
  Planar = 2,

  /// <summary>Length measured along a supplied path geometry.</summary>
  PathLength = 3,

  /// <summary>Distance measured along a routable network or journey.</summary>
  Network = 4,
}

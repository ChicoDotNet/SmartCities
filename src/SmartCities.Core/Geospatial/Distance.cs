namespace SmartCities.Geospatial;

/// <summary>
/// Represents a non-negative distance quantity with explicit derivation semantics.
/// </summary>
/// <remarks>
/// Meters are the canonical stored unit. This value object does not calculate distance from coordinates or paths.
/// </remarks>
public sealed record Distance
{
  private Distance(
    double meters,
    DistanceSemantics semantics)
  {
    Meters = meters;
    Semantics = semantics;
  }

  /// <summary>Gets the distance in canonical meters.</summary>
  public double Meters { get; }

  /// <summary>Gets the distance in kilometers.</summary>
  public double Kilometers =>
    Meters / 1000d;

  /// <summary>Gets the semantics describing how the distance was obtained.</summary>
  public DistanceSemantics Semantics { get; }

  /// <summary>Creates a validated distance stored in meters.</summary>
  public static Distance CreateMeters(
    double meters,
    DistanceSemantics semantics)
  {
    if (!double.IsFinite(meters)
        || meters < 0d)
    {
      throw new ArgumentOutOfRangeException(
        nameof(meters),
        meters,
        "Distance must be finite and non-negative.");
    }

    if (!Enum.IsDefined(semantics))
    {
      throw new ArgumentOutOfRangeException(
        nameof(semantics),
        semantics,
        "Distance semantics must be a defined value.");
    }

    return new Distance(
      meters,
      semantics);
  }
}

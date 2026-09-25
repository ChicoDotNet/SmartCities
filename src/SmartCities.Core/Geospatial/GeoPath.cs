namespace SmartCities.Geospatial;

/// <summary>
/// Represents an ordered provider-neutral path/polyline as points in one coordinate reference system.
/// </summary>
/// <remarks>
/// This primitive preserves geometry supplied by another component. It does not perform routing,
/// topology validation, simplification, projection, or length calculation.
/// </remarks>
public sealed record GeoPath
{
  private GeoPath(
    IReadOnlyList<GeoPoint> points,
    CoordinateReferenceSystem crs)
  {
    Points = points;
    Crs = crs;
  }

  /// <summary>Gets the immutable ordered point collection.</summary>
  public IReadOnlyList<GeoPoint> Points { get; }

  /// <summary>Gets the coordinate reference system shared by every point.</summary>
  public CoordinateReferenceSystem Crs { get; }

  /// <summary>Creates a validated path containing at least two points in one CRS.</summary>
  public static GeoPath Create(
    IEnumerable<GeoPoint> points)
  {
    ArgumentNullException.ThrowIfNull(points);

    var snapshot =
      points.ToArray();

    if (snapshot.Length < 2)
    {
      throw new ArgumentException(
        "A path requires at least two points.",
        nameof(points));
    }

    foreach (var point in snapshot)
    {
      ArgumentNullException.ThrowIfNull(point);
    }

    var crs = snapshot[0].Crs;

    if (snapshot.Any(
        point => point.Crs != crs))
    {
      throw new ArgumentException(
        "Every point in a path must use the same coordinate reference system.",
        nameof(points));
    }

    return new GeoPath(
      Array.AsReadOnly(snapshot),
      crs);
  }
}

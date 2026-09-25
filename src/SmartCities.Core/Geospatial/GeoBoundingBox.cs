namespace SmartCities.Geospatial;

/// <summary>
/// Represents an axis-aligned two-dimensional bounding box in one coordinate reference system.
/// </summary>
/// <remarks>
/// V1.2 requires ordered extents: <c>MinX &lt;= MaxX</c> and <c>MinY &lt;= MaxY</c>.
/// Wrapped antimeridian boxes are therefore intentionally not represented by this primitive.
/// </remarks>
public sealed record GeoBoundingBox
{
  private GeoBoundingBox(
    double minX,
    double minY,
    double maxX,
    double maxY,
    CoordinateReferenceSystem crs)
  {
    MinX = minX;
    MinY = minY;
    MaxX = maxX;
    MaxY = maxY;
    Crs = crs;
  }

  /// <summary>Gets the minimum X coordinate.</summary>
  public double MinX { get; }

  /// <summary>Gets the minimum Y coordinate.</summary>
  public double MinY { get; }

  /// <summary>Gets the maximum X coordinate.</summary>
  public double MaxX { get; }

  /// <summary>Gets the maximum Y coordinate.</summary>
  public double MaxY { get; }

  /// <summary>Gets the coordinate reference system.</summary>
  public CoordinateReferenceSystem Crs { get; }

  /// <summary>Creates a validated bounding box.</summary>
  public static GeoBoundingBox Create(
    double minX,
    double minY,
    double maxX,
    double maxY,
    CoordinateReferenceSystem crs)
  {
    ArgumentNullException.ThrowIfNull(crs);

    _ = GeoPoint.Create(
      minX,
      minY,
      crs);
    _ = GeoPoint.Create(
      maxX,
      maxY,
      crs);

    if (minX > maxX)
    {
      throw new ArgumentException(
        "Bounding-box MinX must be less than or equal to MaxX.",
        nameof(minX));
    }

    if (minY > maxY)
    {
      throw new ArgumentException(
        "Bounding-box MinY must be less than or equal to MaxY.",
        nameof(minY));
    }

    return new GeoBoundingBox(
      minX,
      minY,
      maxX,
      maxY,
      crs);
  }
}

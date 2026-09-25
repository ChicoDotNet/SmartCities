namespace SmartCities.Geospatial;

/// <summary>
/// Represents one provider-neutral two-dimensional point in an explicit coordinate reference system.
/// </summary>
/// <remarks>
/// SmartCities uses canonical X/Y ordering. For <c>EPSG:4326</c>, X is longitude and Y is latitude.
/// External adapters must map provider axis-order conventions into this contract.
/// </remarks>
public sealed record GeoPoint
{
  private GeoPoint(
    double x,
    double y,
    CoordinateReferenceSystem crs)
  {
    X = x;
    Y = y;
    Crs = crs;
  }

  /// <summary>Gets the X coordinate. For WGS 84 this is longitude.</summary>
  public double X { get; }

  /// <summary>Gets the Y coordinate. For WGS 84 this is latitude.</summary>
  public double Y { get; }

  /// <summary>Gets the coordinate reference system.</summary>
  public CoordinateReferenceSystem Crs { get; }

  /// <summary>Creates a validated point.</summary>
  public static GeoPoint Create(
    double x,
    double y,
    CoordinateReferenceSystem crs)
  {
    ArgumentNullException.ThrowIfNull(crs);

    if (!double.IsFinite(x))
    {
      throw new ArgumentOutOfRangeException(
        nameof(x),
        x,
        "Coordinate X must be finite.");
    }

    if (!double.IsFinite(y))
    {
      throw new ArgumentOutOfRangeException(
        nameof(y),
        y,
        "Coordinate Y must be finite.");
    }

    if (crs.IsWgs84)
    {
      if (x < -180d || x > 180d)
      {
        throw new ArgumentOutOfRangeException(
          nameof(x),
          x,
          "WGS 84 longitude must be between -180 and 180 degrees.");
      }

      if (y < -90d || y > 90d)
      {
        throw new ArgumentOutOfRangeException(
          nameof(y),
          y,
          "WGS 84 latitude must be between -90 and 90 degrees.");
      }
    }

    return new GeoPoint(
      x,
      y,
      crs);
  }
}

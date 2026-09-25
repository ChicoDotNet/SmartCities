namespace SmartCities.Geospatial;

/// <summary>
/// References a zone and its externally owned polygon geometry without importing a GIS geometry model.
/// </summary>
public sealed record ZonePolygonReference
{
  private ZonePolygonReference(
    string zoneId,
    string polygonReference,
    CoordinateReferenceSystem crs)
  {
    ZoneId = zoneId;
    PolygonReference = polygonReference;
    Crs = crs;
  }

  /// <summary>Gets the stable SmartCities/municipal zone identifier.</summary>
  public string ZoneId { get; }

  /// <summary>
  /// Gets the provider-neutral reference used to resolve the polygon geometry from City Context or another source.
  /// </summary>
  public string PolygonReference { get; }

  /// <summary>Gets the coordinate reference system expected for the referenced polygon geometry.</summary>
  public CoordinateReferenceSystem Crs { get; }

  /// <summary>Creates a validated zone/polygon reference.</summary>
  public static ZonePolygonReference Create(
    string zoneId,
    string polygonReference,
    CoordinateReferenceSystem crs)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      zoneId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      polygonReference);
    ArgumentNullException.ThrowIfNull(crs);

    return new ZonePolygonReference(
      zoneId,
      polygonReference,
      crs);
  }
}

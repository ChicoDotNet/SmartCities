using SmartCities.Geospatial;

namespace SmartCities.CityContext;

/// <summary>
/// Represents a municipal/analysis zone whose polygon geometry remains externally referenced.
/// </summary>
public sealed record CityZone
{
  private CityZone(
    ZonePolygonReference polygon,
    CityContextProvenance provenance)
  {
    Polygon = polygon;
    Provenance = provenance;
  }

  /// <summary>Gets the stable zone identifier.</summary>
  public string ZoneId =>
    Polygon.ZoneId;

  /// <summary>Gets the provider-neutral polygon reference.</summary>
  public ZonePolygonReference Polygon { get; }

  /// <summary>Gets the source provenance for this zone record.</summary>
  public CityContextProvenance Provenance { get; }

  /// <summary>Creates a validated city zone.</summary>
  public static CityZone Create(
    ZonePolygonReference polygon,
    CityContextProvenance provenance)
  {
    ArgumentNullException.ThrowIfNull(
      polygon);
    ArgumentNullException.ThrowIfNull(
      provenance);

    return new CityZone(
      polygon,
      provenance);
  }
}

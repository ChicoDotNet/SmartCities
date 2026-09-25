using SmartCities.Geospatial;

namespace SmartCities.CityContext;

/// <summary>
/// Represents a provider-neutral destination/point of interest used by Urban Accessibility.
/// </summary>
public sealed record PointOfInterest
{
  private PointOfInterest(
    string pointOfInterestId,
    string categoryId,
    GeoPoint location,
    CityContextProvenance provenance)
  {
    PointOfInterestId = pointOfInterestId;
    CategoryId = categoryId;
    Location = location;
    Provenance = provenance;
  }

  /// <summary>Gets the stable point-of-interest identifier.</summary>
  public string PointOfInterestId { get; }

  /// <summary>Gets the stable destination-category identifier.</summary>
  public string CategoryId { get; }

  /// <summary>Gets the provider-neutral point location.</summary>
  public GeoPoint Location { get; }

  /// <summary>Gets the source provenance for this point of interest.</summary>
  public CityContextProvenance Provenance { get; }

  /// <summary>Creates a validated point of interest.</summary>
  public static PointOfInterest Create(
    string pointOfInterestId,
    string categoryId,
    GeoPoint location,
    CityContextProvenance provenance)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      pointOfInterestId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      categoryId);
    ArgumentNullException.ThrowIfNull(
      location);
    ArgumentNullException.ThrowIfNull(
      provenance);

    return new PointOfInterest(
      pointOfInterestId.Trim(),
      categoryId.Trim(),
      location,
      provenance);
  }
}

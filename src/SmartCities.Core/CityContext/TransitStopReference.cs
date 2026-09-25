using SmartCities.Geospatial;

namespace SmartCities.CityContext;

/// <summary>
/// References a public-transport stop and its location without introducing GTFS or routing-engine types.
/// </summary>
public sealed record TransitStopReference
{
  private TransitStopReference(
    string stopId,
    string networkId,
    string stopReference,
    GeoPoint location,
    CityContextProvenance provenance)
  {
    StopId = stopId;
    NetworkId = networkId;
    StopReference = stopReference;
    Location = location;
    Provenance = provenance;
  }

  /// <summary>Gets the stable SmartCities stop identifier.</summary>
  public string StopId { get; }

  /// <summary>Gets the City Context public-transport network identifier containing this stop.</summary>
  public string NetworkId { get; }

  /// <summary>Gets the source-local reference used to resolve the stop.</summary>
  public string StopReference { get; }

  /// <summary>Gets the provider-neutral stop point.</summary>
  public GeoPoint Location { get; }

  /// <summary>Gets the source provenance for the stop record.</summary>
  public CityContextProvenance Provenance { get; }

  /// <summary>Creates a validated transit-stop reference.</summary>
  public static TransitStopReference Create(
    string stopId,
    string networkId,
    string stopReference,
    GeoPoint location,
    CityContextProvenance provenance)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      stopId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      networkId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      stopReference);
    ArgumentNullException.ThrowIfNull(
      location);
    ArgumentNullException.ThrowIfNull(
      provenance);

    return new TransitStopReference(
      stopId.Trim(),
      networkId.Trim(),
      stopReference.Trim(),
      location,
      provenance);
  }
}

namespace SmartCities.CityContext;

/// <summary>
/// Represents one immutable, internally consistent City Context snapshot for Urban Accessibility.
/// </summary>
/// <remarks>
/// V1.3 owns catalog semantics and referential integrity only. Acquisition, persistence, routing,
/// spatial analysis, localization, and refresh scheduling belong to later observable boundaries.
/// </remarks>
public sealed record CityContextSnapshot
{
  private CityContextSnapshot(
    IReadOnlyList<DestinationCategory> destinationCategories,
    IReadOnlyList<CityZone> zones,
    IReadOnlyList<PointOfInterest> pointsOfInterest,
    IReadOnlyList<MobilityNetworkReference> networks,
    IReadOnlyList<TransitStopReference> transitStops)
  {
    DestinationCategories = destinationCategories;
    Zones = zones;
    PointsOfInterest = pointsOfInterest;
    Networks = networks;
    TransitStops = transitStops;
  }

  /// <summary>Gets the immutable destination-category catalog.</summary>
  public IReadOnlyList<DestinationCategory> DestinationCategories { get; }

  /// <summary>Gets the immutable zone catalog.</summary>
  public IReadOnlyList<CityZone> Zones { get; }

  /// <summary>Gets the immutable destination/point-of-interest catalog.</summary>
  public IReadOnlyList<PointOfInterest> PointsOfInterest { get; }

  /// <summary>Gets the immutable mobility-network reference catalog.</summary>
  public IReadOnlyList<MobilityNetworkReference> Networks { get; }

  /// <summary>Gets the immutable public-transport stop catalog.</summary>
  public IReadOnlyList<TransitStopReference> TransitStops { get; }

  /// <summary>Creates a validated City Context snapshot.</summary>
  public static CityContextSnapshot Create(
    IEnumerable<DestinationCategory> destinationCategories,
    IEnumerable<CityZone> zones,
    IEnumerable<PointOfInterest> pointsOfInterest,
    IEnumerable<MobilityNetworkReference> networks,
    IEnumerable<TransitStopReference> transitStops)
  {
    var categorySnapshot =
      Snapshot(
        destinationCategories,
        nameof(destinationCategories));
    var zoneSnapshot =
      Snapshot(
        zones,
        nameof(zones));
    var pointSnapshot =
      Snapshot(
        pointsOfInterest,
        nameof(pointsOfInterest));
    var networkSnapshot =
      Snapshot(
        networks,
        nameof(networks));
    var stopSnapshot =
      Snapshot(
        transitStops,
        nameof(transitStops));

    EnsureUnique(
      categorySnapshot,
      static value => value.CategoryId,
      "destination category",
      nameof(destinationCategories));
    EnsureUnique(
      zoneSnapshot,
      static value => value.ZoneId,
      "zone",
      nameof(zones));
    EnsureUnique(
      pointSnapshot,
      static value => value.PointOfInterestId,
      "point of interest",
      nameof(pointsOfInterest));
    EnsureUnique(
      networkSnapshot,
      static value => value.NetworkId,
      "network",
      nameof(networks));
    EnsureUnique(
      stopSnapshot,
      static value => value.StopId,
      "transit stop",
      nameof(transitStops));

    var categoryIds =
      categorySnapshot
        .Select(
          static value => value.CategoryId)
        .ToHashSet(
          StringComparer.Ordinal);

    foreach (var point in pointSnapshot)
    {
      if (!categoryIds.Contains(
            point.CategoryId))
      {
        throw new ArgumentException(
          $"Point of interest '{point.PointOfInterestId}' references unknown destination category '{point.CategoryId}'.",
          nameof(pointsOfInterest));
      }
    }

    var networkById =
      networkSnapshot.ToDictionary(
        static value => value.NetworkId,
        StringComparer.Ordinal);

    foreach (var stop in stopSnapshot)
    {
      if (!networkById.TryGetValue(
            stop.NetworkId,
            out var network))
      {
        throw new ArgumentException(
          $"Transit stop '{stop.StopId}' references unknown network '{stop.NetworkId}'.",
          nameof(transitStops));
      }

      if (network.Kind
          != MobilityNetworkKind.PublicTransport)
      {
        throw new ArgumentException(
          $"Transit stop '{stop.StopId}' must reference a public-transport network.",
          nameof(transitStops));
      }
    }

    return new CityContextSnapshot(
      Array.AsReadOnly(categorySnapshot),
      Array.AsReadOnly(zoneSnapshot),
      Array.AsReadOnly(pointSnapshot),
      Array.AsReadOnly(networkSnapshot),
      Array.AsReadOnly(stopSnapshot));
  }

  private static T[] Snapshot<T>(
    IEnumerable<T> values,
    string parameterName)
    where T : class
  {
    ArgumentNullException.ThrowIfNull(
      values,
      parameterName);

    var snapshot =
      values.ToArray();

    if (snapshot.Any(
        static value => value is null))
    {
      throw new ArgumentException(
        "City Context collections cannot contain null entries.",
        parameterName);
    }

    return snapshot;
  }

  private static void EnsureUnique<T>(
    IEnumerable<T> values,
    Func<T, string> identifierSelector,
    string entityName,
    string parameterName)
  {
    var identifiers =
      new HashSet<string>(
        StringComparer.Ordinal);

    foreach (var value in values)
    {
      var identifier =
        identifierSelector(value);

      if (!identifiers.Add(identifier))
      {
        throw new ArgumentException(
          $"Duplicate {entityName} identifier '{identifier}'.",
          parameterName);
      }
    }
  }
}

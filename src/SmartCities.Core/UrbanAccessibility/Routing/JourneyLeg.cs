using SmartCities.Geospatial;

namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Represents one contiguous normalized journey leg.
/// </summary>
public sealed record JourneyLeg
{
  private JourneyLeg(
    JourneyMode mode,
    GeoPoint origin,
    GeoPoint destination,
    DateTimeOffset departureAtUtc,
    DateTimeOffset arrivalAtUtc,
    Distance distance,
    JourneyAccessibility accessibility,
    string? transitServiceReference,
    GeoPath? path)
  {
    Mode = mode;
    Origin = origin;
    Destination = destination;
    DepartureAtUtc = departureAtUtc;
    ArrivalAtUtc = arrivalAtUtc;
    Distance = distance;
    Accessibility = accessibility;
    TransitServiceReference = transitServiceReference;
    Path = path;
  }

  /// <summary>Gets the normalized movement mode.</summary>
  public JourneyMode Mode { get; }

  /// <summary>Gets the leg origin.</summary>
  public GeoPoint Origin { get; }

  /// <summary>Gets the leg destination.</summary>
  public GeoPoint Destination { get; }

  /// <summary>Gets the departure instant normalized to UTC.</summary>
  public DateTimeOffset DepartureAtUtc { get; }

  /// <summary>Gets the arrival instant normalized to UTC.</summary>
  public DateTimeOffset ArrivalAtUtc { get; }

  /// <summary>Gets the leg duration.</summary>
  public TimeSpan Duration =>
    ArrivalAtUtc - DepartureAtUtc;

  /// <summary>Gets the normalized leg distance.</summary>
  public Distance Distance { get; }

  /// <summary>Gets normalized accessibility information for this leg.</summary>
  public JourneyAccessibility Accessibility { get; }

  /// <summary>Gets a provider-neutral public-transport service reference when this is a transit leg.</summary>
  public string? TransitServiceReference { get; }

  /// <summary>Gets optional path geometry when the engine can provide it.</summary>
  public GeoPath? Path { get; }

  /// <summary>Creates a validated journey leg.</summary>
  public static JourneyLeg Create(
    JourneyMode mode,
    GeoPoint origin,
    GeoPoint destination,
    DateTimeOffset departureAt,
    DateTimeOffset arrivalAt,
    Distance distance,
    JourneyAccessibility accessibility,
    string? transitServiceReference = null,
    GeoPath? path = null)
  {
    if (!Enum.IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(
        nameof(mode),
        mode,
        "Journey mode must be a defined value.");
    }

    ArgumentNullException.ThrowIfNull(
      origin);
    ArgumentNullException.ThrowIfNull(
      destination);
    ArgumentNullException.ThrowIfNull(
      distance);
    ArgumentNullException.ThrowIfNull(
      accessibility);

    if (origin.Crs != destination.Crs)
    {
      throw new ArgumentException(
        "Leg origin and destination must use the same coordinate reference system.",
        nameof(destination));
    }

    var departureUtc =
      departureAt.ToUniversalTime();
    var arrivalUtc =
      arrivalAt.ToUniversalTime();

    if (arrivalUtc <= departureUtc)
    {
      throw new ArgumentException(
        "Journey-leg arrival must be later than departure.",
        nameof(arrivalAt));
    }

    if (distance.Semantics is not (
        DistanceSemantics.Network
        or DistanceSemantics.PathLength))
    {
      throw new ArgumentException(
        "Journey-leg distance must use network or path-length semantics.",
        nameof(distance));
    }

    if (mode == JourneyMode.PublicTransport)
    {
      if (string.IsNullOrWhiteSpace(
          transitServiceReference))
      {
        throw new ArgumentException(
          "Public-transport legs require a transit service reference.",
          nameof(transitServiceReference));
      }
    }
    else if (transitServiceReference is not null)
    {
      throw new ArgumentException(
        "Only public-transport legs can carry a transit service reference.",
        nameof(transitServiceReference));
    }

    if (path is not null
        && path.Crs != origin.Crs)
    {
      throw new ArgumentException(
        "Journey-leg path must use the same coordinate reference system as the leg.",
        nameof(path));
    }

    return new JourneyLeg(
      mode,
      origin,
      destination,
      departureUtc,
      arrivalUtc,
      distance,
      accessibility,
      transitServiceReference?.Trim(),
      path);
  }
}

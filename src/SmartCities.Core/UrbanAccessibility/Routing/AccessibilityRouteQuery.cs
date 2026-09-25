using System.Collections.ObjectModel;
using SmartCities.Geospatial;

namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Represents a provider-neutral point-to-point routing request.
/// </summary>
/// <remarks>
/// Destination categories remain a City Context/application concern and are resolved before this port is called.
/// </remarks>
public sealed record AccessibilityRouteQuery
{
  private AccessibilityRouteQuery(
    string requestId,
    GeoPoint origin,
    GeoPoint destination,
    DateTimeOffset departureAtUtc,
    IReadOnlyList<JourneyMode> allowedModes)
  {
    RequestId = requestId;
    Origin = origin;
    Destination = destination;
    DepartureAtUtc = departureAtUtc;
    AllowedModes = allowedModes;
  }

  /// <summary>Gets the stable request identifier.</summary>
  public string RequestId { get; }

  /// <summary>Gets the explicit origin point.</summary>
  public GeoPoint Origin { get; }

  /// <summary>Gets the explicit destination point.</summary>
  public GeoPoint Destination { get; }

  /// <summary>Gets the requested departure instant normalized to UTC.</summary>
  public DateTimeOffset DepartureAtUtc { get; }

  /// <summary>Gets the allowed normalized journey modes.</summary>
  public IReadOnlyList<JourneyMode> AllowedModes { get; }

  /// <summary>Creates a validated point-to-point route query.</summary>
  public static AccessibilityRouteQuery Create(
    string requestId,
    GeoPoint origin,
    GeoPoint destination,
    DateTimeOffset departureAt,
    IEnumerable<JourneyMode> allowedModes)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      requestId);
    ArgumentNullException.ThrowIfNull(
      origin);
    ArgumentNullException.ThrowIfNull(
      destination);
    ArgumentNullException.ThrowIfNull(
      allowedModes);

    if (origin.Crs != destination.Crs)
    {
      throw new ArgumentException(
        "Origin and destination must use the same coordinate reference system.",
        nameof(destination));
    }

    var modes =
      allowedModes.ToArray();

    if (modes.Length == 0)
    {
      throw new ArgumentException(
        "At least one journey mode is required.",
        nameof(allowedModes));
    }

    foreach (var mode in modes)
    {
      if (!Enum.IsDefined(mode))
      {
        throw new ArgumentOutOfRangeException(
          nameof(allowedModes),
          mode,
          "Journey mode must be a defined value.");
      }
    }

    if (modes.Distinct().Count()
        != modes.Length)
    {
      throw new ArgumentException(
        "Allowed journey modes must be unique.",
        nameof(allowedModes));
    }

    return new AccessibilityRouteQuery(
      requestId.Trim(),
      origin,
      destination,
      departureAt.ToUniversalTime(),
      Array.AsReadOnly(modes));
  }
}

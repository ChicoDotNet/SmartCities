namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Represents either successful routing alternatives or one expected provider-neutral failure.
/// </summary>
public sealed record AccessibilityRouteResult
{
  private AccessibilityRouteResult(
    string requestId,
    IReadOnlyList<Journey> journeys,
    AccessibilityRoutingFailure? failure,
    AccessibilityRoutingProvenance provenance)
  {
    RequestId = requestId;
    Journeys = journeys;
    Failure = failure;
    Provenance = provenance;
  }

  /// <summary>Gets the request identifier.</summary>
  public string RequestId { get; }

  /// <summary>Gets the successful journey alternatives, or an empty list when failed.</summary>
  public IReadOnlyList<Journey> Journeys { get; }

  /// <summary>Gets the expected routing failure, or null when successful.</summary>
  public AccessibilityRoutingFailure? Failure { get; }

  /// <summary>Gets routing engine/adapter/source provenance.</summary>
  public AccessibilityRoutingProvenance Provenance { get; }

  /// <summary>Gets whether the result contains successful alternatives.</summary>
  public bool IsSuccess =>
    Failure is null;

  /// <summary>Creates a successful routing result.</summary>
  public static AccessibilityRouteResult Succeeded(
    string requestId,
    IEnumerable<Journey> journeys,
    AccessibilityRoutingProvenance provenance)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      requestId);
    ArgumentNullException.ThrowIfNull(
      journeys);
    ArgumentNullException.ThrowIfNull(
      provenance);

    var snapshot =
      journeys.ToArray();

    if (snapshot.Length == 0)
    {
      throw new ArgumentException(
        "A successful routing result requires at least one journey.",
        nameof(journeys));
    }

    if (snapshot.Any(
        static journey =>
          journey is null))
    {
      throw new ArgumentException(
        "Routing-result journeys cannot contain null entries.",
        nameof(journeys));
    }

    var journeyIds =
      snapshot
        .Select(
          static journey =>
            journey.JourneyId)
        .ToArray();

    if (journeyIds.Distinct(
          StringComparer.Ordinal).Count()
        != journeyIds.Length)
    {
      throw new ArgumentException(
        "Journey identifiers must be unique within one routing result.",
        nameof(journeys));
    }

    return new AccessibilityRouteResult(
      requestId.Trim(),
      Array.AsReadOnly(snapshot),
      failure: null,
      provenance);
  }

  /// <summary>Creates a failed routing result with no journey alternatives.</summary>
  public static AccessibilityRouteResult Failed(
    string requestId,
    AccessibilityRoutingFailure failure,
    AccessibilityRoutingProvenance provenance)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      requestId);
    ArgumentNullException.ThrowIfNull(
      failure);
    ArgumentNullException.ThrowIfNull(
      provenance);

    return new AccessibilityRouteResult(
      requestId.Trim(),
      Array.Empty<Journey>(),
      failure,
      provenance);
  }
}

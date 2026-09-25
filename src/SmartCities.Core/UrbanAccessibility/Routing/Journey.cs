using SmartCities.Geospatial;

namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Represents one normalized routing alternative.
/// </summary>
public sealed record Journey
{
  private Journey(
    string journeyId,
    IReadOnlyList<JourneyLeg> legs,
    TimeSpan totalDuration,
    Distance walkingDistance,
    int transfers,
    JourneyAccessibility accessibility)
  {
    JourneyId = journeyId;
    Legs = legs;
    TotalDuration = totalDuration;
    WalkingDistance = walkingDistance;
    Transfers = transfers;
    Accessibility = accessibility;
  }

  /// <summary>Gets the stable journey-alternative identifier.</summary>
  public string JourneyId { get; }

  /// <summary>Gets the ordered immutable journey legs.</summary>
  public IReadOnlyList<JourneyLeg> Legs { get; }

  /// <summary>Gets elapsed time from first departure to final arrival, including inter-leg waiting.</summary>
  public TimeSpan TotalDuration { get; }

  /// <summary>Gets total walking distance using the common walking-leg distance semantics.</summary>
  public Distance WalkingDistance { get; }

  /// <summary>Gets public-transport transfers. Walking access/egress does not count as a transfer.</summary>
  public int Transfers { get; }

  /// <summary>Gets the conservative aggregate accessibility state across all legs.</summary>
  public JourneyAccessibility Accessibility { get; }

  /// <summary>Creates a validated journey and derives aggregate values from its legs.</summary>
  public static Journey Create(
    string journeyId,
    IEnumerable<JourneyLeg> legs)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      journeyId);
    ArgumentNullException.ThrowIfNull(
      legs);

    var snapshot =
      legs.ToArray();

    if (snapshot.Length == 0)
    {
      throw new ArgumentException(
        "A journey requires at least one leg.",
        nameof(legs));
    }

    if (snapshot.Any(
        static leg => leg is null))
    {
      throw new ArgumentException(
        "Journey legs cannot contain null entries.",
        nameof(legs));
    }

    for (var index = 1; index < snapshot.Length; index++)
    {
      if (snapshot[index].DepartureAtUtc
          < snapshot[index - 1].ArrivalAtUtc)
      {
        throw new ArgumentException(
          "Journey legs cannot overlap in time.",
          nameof(legs));
      }
    }

    var walkingLegs =
      snapshot
        .Where(
          static leg =>
            leg.Mode == JourneyMode.Walking)
        .ToArray();

    Distance walkingDistance;

    if (walkingLegs.Length == 0)
    {
      walkingDistance =
        Distance.CreateMeters(
          0,
          DistanceSemantics.Network);
    }
    else
    {
      var walkingSemantics =
        walkingLegs[0]
          .Distance
          .Semantics;

      if (walkingLegs.Any(
          leg =>
            leg.Distance.Semantics
            != walkingSemantics))
      {
        throw new ArgumentException(
          "All walking legs in one journey must use the same distance semantics.",
          nameof(legs));
      }

      walkingDistance =
        Distance.CreateMeters(
          walkingLegs.Sum(
            static leg =>
              leg.Distance.Meters),
          walkingSemantics);
    }

    var transitLegCount =
      snapshot.Count(
        static leg =>
          leg.Mode
          == JourneyMode.PublicTransport);

    var totalDuration =
      snapshot[^1].ArrivalAtUtc
      - snapshot[0].DepartureAtUtc;

    return new Journey(
      journeyId.Trim(),
      Array.AsReadOnly(snapshot),
      totalDuration,
      walkingDistance,
      Math.Max(
        0,
        transitLegCount - 1),
      AggregateAccessibility(snapshot));
  }

  private static JourneyAccessibility AggregateAccessibility(
    IEnumerable<JourneyLeg> legs)
  {
    var accessibility =
      legs
        .Select(
          static leg =>
            leg.Accessibility)
        .ToArray();

    var limitations =
      accessibility
        .SelectMany(
          static value =>
            value.LimitationCodes)
        .Distinct(
          StringComparer.Ordinal)
        .ToArray();

    if (accessibility.Any(
        static value =>
          value.Status
          == AccessibilityStatus.KnownInaccessible))
    {
      return JourneyAccessibility.KnownInaccessible(
        limitations);
    }

    if (accessibility.Any(
        static value =>
          value.Status
          == AccessibilityStatus.Unknown))
    {
      return JourneyAccessibility.Unknown();
    }

    if (accessibility.Any(
        static value =>
          value.Status
          == AccessibilityStatus.KnownLimited))
    {
      return JourneyAccessibility.KnownLimited(
        limitations);
    }

    return JourneyAccessibility.KnownAccessible();
  }
}

using SmartCities.CityContext;
using SmartCities.Geospatial;
using SmartCities.UrbanAccessibility.Routing;
using Xunit;

namespace SmartCities.Core.Tests.UrbanAccessibility.Routing;

public sealed class AccessibilityRoutingContractTests
{
  [Fact]
  public void Route_query_preserves_provider_neutral_points_modes_and_utc_departure()
  {
    var departure =
      new DateTimeOffset(
        2026,
        9,
        25,
        8,
        30,
        0,
        TimeSpan.FromHours(-6));

    var query =
      AccessibilityRouteQuery.Create(
        "route-request-001",
        Point(-100.0050, 20.0140),
        Point(-99.9960, 19.9880),
        departure,
        [
          JourneyMode.Walking,
          JourneyMode.PublicTransport,
        ]);

    Assert.Equal(
      "route-request-001",
      query.RequestId);
    Assert.Equal(
      departure.ToUniversalTime(),
      query.DepartureAtUtc);
    Assert.Equal(
      [
        JourneyMode.Walking,
        JourneyMode.PublicTransport,
      ],
      query.AllowedModes);
  }

  [Fact]
  public void Route_query_requires_same_crs_and_at_least_one_unique_defined_mode()
  {
    Assert.Throws<ArgumentException>(
      () => AccessibilityRouteQuery.Create(
        "request",
        Point(-100.0050, 20.0140),
        GeoPoint.Create(
          500_000,
          2_420_000,
          CoordinateReferenceSystem.Epsg(32614)),
        Instant(8, 30),
        [JourneyMode.Walking]));

    Assert.Throws<ArgumentException>(
      () => AccessibilityRouteQuery.Create(
        "request",
        Point(-100.0050, 20.0140),
        Point(-99.9960, 19.9880),
        Instant(8, 30),
        []));

    Assert.Throws<ArgumentException>(
      () => AccessibilityRouteQuery.Create(
        "request",
        Point(-100.0050, 20.0140),
        Point(-99.9960, 19.9880),
        Instant(8, 30),
        [
          JourneyMode.Walking,
          JourneyMode.Walking,
        ]));

    Assert.Throws<ArgumentOutOfRangeException>(
      () => AccessibilityRouteQuery.Create(
        "request",
        Point(-100.0050, 20.0140),
        Point(-99.9960, 19.9880),
        Instant(8, 30),
        [(JourneyMode)999]));
  }

  [Fact]
  public void Execution_options_require_a_positive_timeout()
  {
    var options =
      AccessibilityRoutingExecutionOptions.Create(
        TimeSpan.FromSeconds(8));

    Assert.Equal(
      TimeSpan.FromSeconds(8),
      options.Timeout);

    Assert.Throws<ArgumentOutOfRangeException>(
      () => AccessibilityRoutingExecutionOptions.Create(
        TimeSpan.Zero));
    Assert.Throws<ArgumentOutOfRangeException>(
      () => AccessibilityRoutingExecutionOptions.Create(
        TimeSpan.FromSeconds(-1)));
    Assert.Throws<ArgumentOutOfRangeException>(
      () => AccessibilityRoutingExecutionOptions.Create(
        Timeout.InfiniteTimeSpan));
  }

  [Fact]
  public void Capabilities_are_explicit_and_defensively_copied()
  {
    var modes =
      new List<JourneyMode>
      {
        JourneyMode.Walking,
        JourneyMode.PublicTransport,
      };

    var capabilities =
      AccessibilityRoutingCapabilities.Create(
        "routing-engine",
        modes,
        supportsScheduledTransit: true,
        supportsAccessibilityInformation: false,
        supportsPathGeometry: true);

    modes.Clear();

    Assert.Equal(
      "routing-engine",
      capabilities.EngineId);
    Assert.Equal(
      [
        JourneyMode.Walking,
        JourneyMode.PublicTransport,
      ],
      capabilities.SupportedModes);
    Assert.True(
      capabilities.SupportsScheduledTransit);
    Assert.False(
      capabilities.SupportsAccessibilityInformation);
    Assert.True(
      capabilities.SupportsPathGeometry);
  }

  [Fact]
  public void Capabilities_reject_duplicate_or_undefined_modes()
  {
    Assert.Throws<ArgumentException>(
      () => AccessibilityRoutingCapabilities.Create(
        "engine",
        [
          JourneyMode.Walking,
          JourneyMode.Walking,
        ],
        false,
        false,
        false));

    Assert.Throws<ArgumentOutOfRangeException>(
      () => AccessibilityRoutingCapabilities.Create(
        "engine",
        [(JourneyMode)999],
        false,
        false,
        false));
  }

  [Fact]
  public void Accessibility_information_preserves_unknown_without_inventing_limitations()
  {
    var unknown =
      JourneyAccessibility.Unknown();

    Assert.Equal(
      AccessibilityStatus.Unknown,
      unknown.Status);
    Assert.Empty(
      unknown.LimitationCodes);

    Assert.Throws<ArgumentException>(
      () => JourneyAccessibility.Create(
        AccessibilityStatus.Unknown,
        ["unknown-but-invented"]));
  }

  [Fact]
  public void Known_limited_accessibility_requires_non_localized_limitation_codes()
  {
    var limited =
      JourneyAccessibility.Create(
        AccessibilityStatus.KnownLimited,
        [
          "stairs-required",
          "elevator-unavailable",
        ]);

    Assert.Equal(
      AccessibilityStatus.KnownLimited,
      limited.Status);
    Assert.Equal(
      [
        "stairs-required",
        "elevator-unavailable",
      ],
      limited.LimitationCodes);

    Assert.Throws<ArgumentException>(
      () => JourneyAccessibility.Create(
        AccessibilityStatus.KnownLimited,
        []));
  }

  [Theory]
  [InlineData(DistanceSemantics.Geodesic)]
  [InlineData(DistanceSemantics.Planar)]
  public void Journey_leg_rejects_straight_line_distance_semantics(
    DistanceSemantics semantics)
  {
    Assert.Throws<ArgumentException>(
      () => JourneyLeg.Create(
        JourneyMode.Walking,
        Point(-100.0050, 20.0140),
        Point(-100.0000, 20.0100),
        Instant(8, 30),
        Instant(8, 36),
        Distance.CreateMeters(
          520,
          semantics),
        JourneyAccessibility.KnownAccessible()));
  }

  [Fact]
  public void Walking_leg_has_no_transit_service_reference()
  {
    var leg =
      JourneyLeg.Create(
        JourneyMode.Walking,
        Point(-100.0050, 20.0140),
        Point(-100.0000, 20.0100),
        Instant(8, 30),
        Instant(8, 36),
        Distance.CreateMeters(
          520,
          DistanceSemantics.Network),
        JourneyAccessibility.KnownAccessible());

    Assert.Equal(
      JourneyMode.Walking,
      leg.Mode);
    Assert.Null(
      leg.TransitServiceReference);
    Assert.Equal(
      TimeSpan.FromMinutes(6),
      leg.Duration);
  }

  [Fact]
  public void Transit_leg_requires_a_provider_neutral_service_reference()
  {
    Assert.Throws<ArgumentException>(
      () => JourneyLeg.Create(
        JourneyMode.PublicTransport,
        Point(-100.0000, 20.0100),
        Point(-100.0000, 20.0000),
        Instant(8, 40),
        Instant(8, 50),
        Distance.CreateMeters(
          1_100,
          DistanceSemantics.Network),
        JourneyAccessibility.Unknown(),
        transitServiceReference: null));

    var leg =
      JourneyLeg.Create(
        JourneyMode.PublicTransport,
        Point(-100.0000, 20.0100),
        Point(-100.0000, 20.0000),
        Instant(8, 40),
        Instant(8, 50),
        Distance.CreateMeters(
          1_100,
          DistanceSemantics.Network),
        JourneyAccessibility.Unknown(),
        transitServiceReference:
          "demo-town:route-1:trip-southbound");

    Assert.Equal(
      "demo-town:route-1:trip-southbound",
      leg.TransitServiceReference);
  }

  [Fact]
  public void Leg_rejects_non_positive_time_and_incompatible_path_crs()
  {
    Assert.Throws<ArgumentException>(
      () => JourneyLeg.Create(
        JourneyMode.Walking,
        Point(-100.0050, 20.0140),
        Point(-100.0000, 20.0100),
        Instant(8, 30),
        Instant(8, 30),
        Distance.CreateMeters(
          520,
          DistanceSemantics.Network),
        JourneyAccessibility.KnownAccessible()));

    var projectedPath =
      GeoPath.Create(
        [
          GeoPoint.Create(
            500_000,
            2_420_000,
            CoordinateReferenceSystem.Epsg(32614)),
          GeoPoint.Create(
            500_200,
            2_419_800,
            CoordinateReferenceSystem.Epsg(32614)),
        ]);

    Assert.Throws<ArgumentException>(
      () => JourneyLeg.Create(
        JourneyMode.Walking,
        Point(-100.0050, 20.0140),
        Point(-100.0000, 20.0100),
        Instant(8, 30),
        Instant(8, 36),
        Distance.CreateMeters(
          520,
          DistanceSemantics.Network),
        JourneyAccessibility.KnownAccessible(),
        path: projectedPath));
  }

  [Fact]
  public void Journey_derives_duration_walking_distance_transfers_and_accessibility()
  {
    var journey =
      Journey.Create(
        "journey-001",
        [
          WalkingLeg(
            Instant(8, 30),
            Instant(8, 36),
            520,
            JourneyAccessibility.KnownAccessible()),
          TransitLeg(
            Instant(8, 40),
            Instant(8, 50),
            "trip-a",
            JourneyAccessibility.Unknown()),
          WalkingLeg(
            Instant(8, 51),
            Instant(8, 54),
            210,
            JourneyAccessibility.KnownAccessible()),
        ]);

    Assert.Equal(
      TimeSpan.FromMinutes(24),
      journey.TotalDuration);
    Assert.Equal(
      730,
      journey.WalkingDistance.Meters);
    Assert.Equal(
      DistanceSemantics.Network,
      journey.WalkingDistance.Semantics);
    Assert.Equal(
      0,
      journey.Transfers);
    Assert.Equal(
      AccessibilityStatus.Unknown,
      journey.Accessibility.Status);
  }

  [Fact]
  public void Walking_access_and_egress_do_not_count_as_transfers()
  {
    var journey =
      Journey.Create(
        "journey-002",
        [
          WalkingLeg(
            Instant(8, 30),
            Instant(8, 35),
            400,
            JourneyAccessibility.KnownAccessible()),
          TransitLeg(
            Instant(8, 40),
            Instant(8, 50),
            "trip-a",
            JourneyAccessibility.KnownAccessible()),
          WalkingLeg(
            Instant(8, 51),
            Instant(8, 54),
            200,
            JourneyAccessibility.KnownAccessible()),
        ]);

    Assert.Equal(
      0,
      journey.Transfers);
  }

  [Fact]
  public void A_second_transit_leg_counts_as_one_transfer()
  {
    var journey =
      Journey.Create(
        "journey-003",
        [
          TransitLeg(
            Instant(8, 30),
            Instant(8, 40),
            "trip-a",
            JourneyAccessibility.KnownAccessible()),
          WalkingLeg(
            Instant(8, 40),
            Instant(8, 43),
            150,
            JourneyAccessibility.KnownAccessible()),
          TransitLeg(
            Instant(8, 45),
            Instant(8, 55),
            "trip-b",
            JourneyAccessibility.KnownLimited(
              "elevator-unavailable")),
        ]);

    Assert.Equal(
      1,
      journey.Transfers);
    Assert.Equal(
      AccessibilityStatus.KnownLimited,
      journey.Accessibility.Status);
    Assert.Contains(
      "elevator-unavailable",
      journey.Accessibility.LimitationCodes);
  }

  [Fact]
  public void Journey_rejects_overlapping_legs_and_mixed_walking_distance_semantics()
  {
    Assert.Throws<ArgumentException>(
      () => Journey.Create(
        "journey",
        [
          WalkingLeg(
            Instant(8, 30),
            Instant(8, 40),
            500,
            JourneyAccessibility.KnownAccessible()),
          WalkingLeg(
            Instant(8, 39),
            Instant(8, 45),
            300,
            JourneyAccessibility.KnownAccessible()),
        ]));

    Assert.Throws<ArgumentException>(
      () => Journey.Create(
        "journey",
        [
          WalkingLeg(
            Instant(8, 30),
            Instant(8, 40),
            500,
            JourneyAccessibility.KnownAccessible()),
          JourneyLeg.Create(
            JourneyMode.Walking,
            Point(-100.0000, 20.0100),
            Point(-100.0000, 20.0000),
            Instant(8, 41),
            Instant(8, 45),
            Distance.CreateMeters(
              300,
              DistanceSemantics.PathLength),
            JourneyAccessibility.KnownAccessible()),
        ]));
  }

  [Fact]
  public void Routing_provenance_preserves_engine_adapter_and_source_versions()
  {
    var sources =
      new List<CityContextProvenance>
      {
        Source(
          "transit/gtfs",
          "v1"),
        Source(
          "networks/pedestrian.geojson",
          "v1"),
      };

    var provenance =
      AccessibilityRoutingProvenance.Create(
        "engine",
        engineVersion: "2.0",
        "adapter",
        "1.0",
        sources,
        Instant(9, 0));

    sources.Clear();

    Assert.Equal(
      "engine",
      provenance.EngineId);
    Assert.Equal(
      "2.0",
      provenance.EngineVersion);
    Assert.Equal(
      "adapter",
      provenance.AdapterId);
    Assert.Equal(
      "1.0",
      provenance.AdapterVersion);
    Assert.Equal(
      2,
      provenance.Sources.Count);
    Assert.Equal(
      Instant(9, 0).ToUniversalTime(),
      provenance.GeneratedAtUtc);
  }

  [Fact]
  public void Successful_result_requires_at_least_one_journey_and_no_failure()
  {
    var result =
      AccessibilityRouteResult.Succeeded(
        "request-001",
        [
          Journey.Create(
            "journey-001",
            [
              WalkingLeg(
                Instant(8, 30),
                Instant(8, 35),
                400,
                JourneyAccessibility.KnownAccessible()),
            ]),
        ],
        Provenance());

    Assert.True(
      result.IsSuccess);
    Assert.Single(
      result.Journeys);
    Assert.Null(
      result.Failure);

    Assert.Throws<ArgumentException>(
      () => AccessibilityRouteResult.Succeeded(
        "request-001",
        [],
        Provenance()));
  }

  [Theory]
  [InlineData(AccessibilityRoutingFailureCode.NoRoute)]
  [InlineData(AccessibilityRoutingFailureCode.UnsupportedRequest)]
  [InlineData(AccessibilityRoutingFailureCode.SourceDataUnavailable)]
  [InlineData(AccessibilityRoutingFailureCode.SourceDataInvalid)]
  [InlineData(AccessibilityRoutingFailureCode.EngineUnavailable)]
  [InlineData(AccessibilityRoutingFailureCode.TimedOut)]
  [InlineData(AccessibilityRoutingFailureCode.EngineFailure)]
  public void Failed_result_preserves_provider_neutral_failure_taxonomy(
    AccessibilityRoutingFailureCode code)
  {
    var result =
      AccessibilityRouteResult.Failed(
        "request-001",
        AccessibilityRoutingFailure.Create(
          code),
        Provenance());

    Assert.False(
      result.IsSuccess);
    Assert.Empty(
      result.Journeys);
    Assert.NotNull(
      result.Failure);
    Assert.Equal(
      code,
      result.Failure.Code);
  }

  [Fact]
  public async Task Port_exposes_capability_discovery_execution_timeout_and_caller_cancellation()
  {
    using var cancellation =
      new CancellationTokenSource();
    var engine =
      new ContractProbeRoutingEngine();

    var capabilities =
      await engine.GetCapabilitiesAsync(
        cancellation.Token);

    var options =
      AccessibilityRoutingExecutionOptions.Create(
        TimeSpan.FromSeconds(5));
    var query =
      AccessibilityRouteQuery.Create(
        "request-001",
        Point(-100.0050, 20.0140),
        Point(-99.9960, 19.9880),
        Instant(8, 30),
        [
          JourneyMode.Walking,
          JourneyMode.PublicTransport,
        ]);

    _ = await engine.RouteAsync(
      query,
      options,
      cancellation.Token);

    Assert.Equal(
      cancellation.Token,
      engine.CapabilitiesCancellationToken);
    Assert.Equal(
      cancellation.Token,
      engine.RouteCancellationToken);
    Assert.Equal(
      options,
      engine.ExecutionOptions);
    Assert.Contains(
      JourneyMode.PublicTransport,
      capabilities.SupportedModes);
  }

  private static GeoPoint Point(
    double x,
    double y) =>
    GeoPoint.Create(
      x,
      y,
      CoordinateReferenceSystem.Wgs84);

  private static DateTimeOffset Instant(
    int hour,
    int minute) =>
    new(
      2026,
      9,
      25,
      hour,
      minute,
      0,
      TimeSpan.Zero);

  private static JourneyLeg WalkingLeg(
    DateTimeOffset departure,
    DateTimeOffset arrival,
    double meters,
    JourneyAccessibility accessibility) =>
    JourneyLeg.Create(
      JourneyMode.Walking,
      Point(-100.0050, 20.0140),
      Point(-100.0000, 20.0100),
      departure,
      arrival,
      Distance.CreateMeters(
        meters,
        DistanceSemantics.Network),
      accessibility);

  private static JourneyLeg TransitLeg(
    DateTimeOffset departure,
    DateTimeOffset arrival,
    string serviceReference,
    JourneyAccessibility accessibility) =>
    JourneyLeg.Create(
      JourneyMode.PublicTransport,
      Point(-100.0000, 20.0100),
      Point(-100.0000, 20.0000),
      departure,
      arrival,
      Distance.CreateMeters(
        1_100,
        DistanceSemantics.Network),
      accessibility,
      transitServiceReference:
        serviceReference);

  private static CityContextProvenance Source(
    string sourceReference,
    string? version) =>
    CityContextProvenance.Create(
      "demo-town",
      sourceReference,
      version,
      Instant(0, 0),
      Instant(0, 0));

  private static AccessibilityRoutingProvenance Provenance() =>
    AccessibilityRoutingProvenance.Create(
      "test-engine",
      "1.0",
      "test-adapter",
      "1.0",
      [
        Source(
          "transit/gtfs",
          "v1"),
        Source(
          "networks/pedestrian.geojson",
          "v1"),
      ],
      Instant(9, 0));

  private sealed class ContractProbeRoutingEngine
    : IAccessibilityRoutingEngine
  {
    public CancellationToken CapabilitiesCancellationToken { get; private set; }

    public CancellationToken RouteCancellationToken { get; private set; }

    public AccessibilityRoutingExecutionOptions? ExecutionOptions { get; private set; }

    public Task<AccessibilityRoutingCapabilities> GetCapabilitiesAsync(
      CancellationToken cancellationToken = default)
    {
      CapabilitiesCancellationToken =
        cancellationToken;

      return Task.FromResult(
        AccessibilityRoutingCapabilities.Create(
          "probe",
          [
            JourneyMode.Walking,
            JourneyMode.PublicTransport,
          ],
          supportsScheduledTransit: true,
          supportsAccessibilityInformation: true,
          supportsPathGeometry: false));
    }

    public Task<AccessibilityRouteResult> RouteAsync(
      AccessibilityRouteQuery query,
      AccessibilityRoutingExecutionOptions options,
      CancellationToken cancellationToken = default)
    {
      ArgumentNullException.ThrowIfNull(
        query);
      ArgumentNullException.ThrowIfNull(
        options);

      RouteCancellationToken =
        cancellationToken;
      ExecutionOptions =
        options;

      return Task.FromResult(
        AccessibilityRouteResult.Failed(
          query.RequestId,
          AccessibilityRoutingFailure.Create(
            AccessibilityRoutingFailureCode.NoRoute),
          Provenance()));
    }
  }
}

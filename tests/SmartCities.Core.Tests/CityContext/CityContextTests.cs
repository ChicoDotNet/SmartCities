using SmartCities.CityContext;
using SmartCities.Geospatial;
using Xunit;

namespace SmartCities.Core.Tests.CityContext;

public sealed class CityContextTests
{
  [Fact]
  public void Provenance_preserves_source_version_and_freshness_without_inventing_unknowns()
  {
    var retrievedAt =
      new DateTimeOffset(
        2026,
        9,
        24,
        18,
        0,
        0,
        TimeSpan.FromHours(-6));
    var effectiveAt =
      new DateTimeOffset(
        2026,
        9,
        20,
        0,
        0,
        0,
        TimeSpan.FromHours(-6));

    var provenance = CityContextProvenance.Create(
      "municipal-open-data",
      "zones-2026",
      "v3",
      effectiveAt,
      retrievedAt);

    Assert.Equal(
      "municipal-open-data",
      provenance.SourceSystem);
    Assert.Equal(
      "zones-2026",
      provenance.SourceReference);
    Assert.Equal(
      "v3",
      provenance.SourceVersion);
    Assert.Equal(
      effectiveAt.ToUniversalTime(),
      provenance.EffectiveAtUtc);
    Assert.Equal(
      retrievedAt.ToUniversalTime(),
      provenance.RetrievedAtUtc);

    var unknownFreshness =
      CityContextProvenance.Create(
        "manual-import",
        "destinations.csv",
        sourceVersion: null,
        effectiveAt: null,
        retrievedAt);

    Assert.Null(
      unknownFreshness.SourceVersion);
    Assert.Null(
      unknownFreshness.EffectiveAtUtc);
  }

  [Fact]
  public void Provenance_rejects_missing_required_source_identity_or_blank_version()
  {
    var retrievedAt =
      new DateTimeOffset(
        2026,
        9,
        24,
        18,
        0,
        0,
        TimeSpan.Zero);

    Assert.Throws<ArgumentException>(
      () => CityContextProvenance.Create(
        " ",
        "zones",
        null,
        null,
        retrievedAt));
    Assert.Throws<ArgumentException>(
      () => CityContextProvenance.Create(
        "municipal-open-data",
        " ",
        null,
        null,
        retrievedAt));
    Assert.Throws<ArgumentException>(
      () => CityContextProvenance.Create(
        "municipal-open-data",
        "zones",
        " ",
        null,
        retrievedAt));
  }

  [Fact]
  public void Destination_category_is_a_stable_non_localized_key()
  {
    var category =
      DestinationCategory.Create(
        "healthcare");

    Assert.Equal(
      "healthcare",
      category.CategoryId);

    Assert.Throws<ArgumentException>(
      () => DestinationCategory.Create(" "));
  }

  [Fact]
  public void City_zone_reuses_external_polygon_reference_and_provenance()
  {
    var provenance =
      Provenance("zones");
    var polygon =
      ZonePolygonReference.Create(
        "zone-centro",
        "demo-town:zones:centro:v1",
        CoordinateReferenceSystem.Wgs84);

    var zone =
      CityZone.Create(
        polygon,
        provenance);

    Assert.Equal(
      "zone-centro",
      zone.ZoneId);
    Assert.Same(
      polygon,
      zone.Polygon);
    Assert.Same(
      provenance,
      zone.Provenance);
  }

  [Fact]
  public void Point_of_interest_preserves_category_location_and_provenance()
  {
    var location =
      GeoPoint.Create(
        -102.2916,
        21.8818,
        CoordinateReferenceSystem.Wgs84);
    var provenance =
      Provenance("destinations");

    var destination =
      PointOfInterest.Create(
        "poi-hospital-01",
        "healthcare",
        location,
        provenance);

    Assert.Equal(
      "poi-hospital-01",
      destination.PointOfInterestId);
    Assert.Equal(
      "healthcare",
      destination.CategoryId);
    Assert.Same(
      location,
      destination.Location);
    Assert.Same(
      provenance,
      destination.Provenance);
  }

  [Theory]
  [InlineData(MobilityNetworkKind.Pedestrian)]
  [InlineData(MobilityNetworkKind.PublicTransport)]
  public void Mobility_network_reference_is_provider_neutral(
    MobilityNetworkKind kind)
  {
    var network =
      MobilityNetworkReference.Create(
        "network-01",
        kind,
        "demo-town:network:01",
        Provenance("networks"));

    Assert.Equal(
      "network-01",
      network.NetworkId);
    Assert.Equal(
      kind,
      network.Kind);
    Assert.Equal(
      "demo-town:network:01",
      network.NetworkReference);
  }

  [Fact]
  public void Mobility_network_reference_rejects_undefined_kind()
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => MobilityNetworkReference.Create(
        "network-01",
        (MobilityNetworkKind)999,
        "demo-town:network:01",
        Provenance("networks")));
  }

  [Fact]
  public void Transit_stop_reference_preserves_network_location_and_provenance()
  {
    var location =
      GeoPoint.Create(
        -102.2900,
        21.8820,
        CoordinateReferenceSystem.Wgs84);

    var stop =
      TransitStopReference.Create(
        "stop-01",
        "transit-network",
        "demo-town:stops:01",
        location,
        Provenance("stops"));

    Assert.Equal(
      "stop-01",
      stop.StopId);
    Assert.Equal(
      "transit-network",
      stop.NetworkId);
    Assert.Equal(
      "demo-town:stops:01",
      stop.StopReference);
    Assert.Same(
      location,
      stop.Location);
  }

  [Fact]
  public void Snapshot_preserves_consistent_city_context_and_defensively_copies()
  {
    var categories =
      new List<DestinationCategory>
      {
        DestinationCategory.Create(
          "healthcare"),
      };
    var zones =
      new List<CityZone>
      {
        Zone("zone-centro"),
      };
    var destinations =
      new List<PointOfInterest>
      {
        Destination(
          "poi-hospital-01",
          "healthcare"),
      };
    var networks =
      new List<MobilityNetworkReference>
      {
        Network(
          "pedestrian-network",
          MobilityNetworkKind.Pedestrian),
        Network(
          "transit-network",
          MobilityNetworkKind.PublicTransport),
      };
    var stops =
      new List<TransitStopReference>
      {
        Stop(
          "stop-01",
          "transit-network"),
      };

    var snapshot =
      CityContextSnapshot.Create(
        categories,
        zones,
        destinations,
        networks,
        stops);

    categories.Add(
      DestinationCategory.Create(
        "education"));
    zones.Add(
      Zone("zone-norte"));
    destinations.Clear();
    networks.Clear();
    stops.Clear();

    Assert.Single(
      snapshot.DestinationCategories);
    Assert.Single(
      snapshot.Zones);
    Assert.Single(
      snapshot.PointsOfInterest);
    Assert.Equal(
      2,
      snapshot.Networks.Count);
    Assert.Single(
      snapshot.TransitStops);
  }

  [Fact]
  public void Snapshot_rejects_duplicate_identifiers_within_each_catalog()
  {
    Assert.Throws<ArgumentException>(
      () => CityContextSnapshot.Create(
        [
          DestinationCategory.Create(
            "healthcare"),
          DestinationCategory.Create(
            "healthcare"),
        ],
        [],
        [],
        [],
        []));

    Assert.Throws<ArgumentException>(
      () => CityContextSnapshot.Create(
        [],
        [
          Zone("zone-centro"),
          Zone("zone-centro"),
        ],
        [],
        [],
        []));

    Assert.Throws<ArgumentException>(
      () => CityContextSnapshot.Create(
        [],
        [],
        [],
        [
          Network(
            "network-01",
            MobilityNetworkKind.Pedestrian),
          Network(
            "network-01",
            MobilityNetworkKind.PublicTransport),
        ],
        []));
  }

  [Fact]
  public void Snapshot_rejects_destination_with_unknown_category()
  {
    Assert.Throws<ArgumentException>(
      () => CityContextSnapshot.Create(
        [
          DestinationCategory.Create(
            "healthcare"),
        ],
        [],
        [
          Destination(
            "poi-school-01",
            "education"),
        ],
        [],
        []));
  }

  [Fact]
  public void Snapshot_rejects_transit_stop_with_unknown_network()
  {
    Assert.Throws<ArgumentException>(
      () => CityContextSnapshot.Create(
        [],
        [],
        [],
        [],
        [
          Stop(
            "stop-01",
            "missing-network"),
        ]));
  }

  [Fact]
  public void Snapshot_rejects_transit_stop_attached_to_non_transit_network()
  {
    Assert.Throws<ArgumentException>(
      () => CityContextSnapshot.Create(
        [],
        [],
        [],
        [
          Network(
            "pedestrian-network",
            MobilityNetworkKind.Pedestrian),
        ],
        [
          Stop(
            "stop-01",
            "pedestrian-network"),
        ]));
  }

  private static CityContextProvenance Provenance(
    string sourceReference) =>
    CityContextProvenance.Create(
      "demo-town",
      sourceReference,
      "v1",
      new DateTimeOffset(
        2026,
        9,
        1,
        0,
        0,
        0,
        TimeSpan.Zero),
      new DateTimeOffset(
        2026,
        9,
        24,
        18,
        0,
        0,
        TimeSpan.Zero));

  private static CityZone Zone(
    string zoneId) =>
    CityZone.Create(
      ZonePolygonReference.Create(
        zoneId,
        $"demo-town:zones:{zoneId}:v1",
        CoordinateReferenceSystem.Wgs84),
      Provenance("zones"));

  private static PointOfInterest Destination(
    string id,
    string categoryId) =>
    PointOfInterest.Create(
      id,
      categoryId,
      GeoPoint.Create(
        -102.2916,
        21.8818,
        CoordinateReferenceSystem.Wgs84),
      Provenance("destinations"));

  private static MobilityNetworkReference Network(
    string id,
    MobilityNetworkKind kind) =>
    MobilityNetworkReference.Create(
      id,
      kind,
      $"demo-town:networks:{id}:v1",
      Provenance("networks"));

  private static TransitStopReference Stop(
    string id,
    string networkId) =>
    TransitStopReference.Create(
      id,
      networkId,
      $"demo-town:stops:{id}:v1",
      GeoPoint.Create(
        -102.2900,
        21.8820,
        CoordinateReferenceSystem.Wgs84),
      Provenance("stops"));
}

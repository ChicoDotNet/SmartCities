using System.Text.Json;
using SmartCities.CityContext;
using SmartCities.Geospatial;
using Xunit;

namespace SmartCities.Core.Tests.DemoTown;

public sealed class DemoTownSampleTests
{
  private static readonly string SampleRoot =
    Path.Combine(
      AppContext.BaseDirectory,
      "samples",
      "demo-town");

  [Fact]
  public void Demo_town_distribution_contains_the_documented_synthetic_dataset()
  {
    Assert.True(
      Directory.Exists(SampleRoot),
      $"Demo Town sample directory was not copied to '{SampleRoot}'.");

    var requiredFiles = new[]
    {
      "README.md",
      "city-context.json",
      "geography/boundary.geojson",
      "geography/zones.geojson",
      "geography/pois.geojson",
      "networks/pedestrian.geojson",
      "transit/gtfs/agency.txt",
      "transit/gtfs/stops.txt",
      "transit/gtfs/routes.txt",
      "transit/gtfs/trips.txt",
      "transit/gtfs/stop_times.txt",
      "transit/gtfs/calendar.txt",
      "transit/gtfs/shapes.txt",
      "transit/gtfs/feed_info.txt",
    };

    foreach (var relativePath in requiredFiles)
    {
      Assert.True(
        File.Exists(
          Path.Combine(
            SampleRoot,
            relativePath)),
        $"Demo Town is missing '{relativePath}'.");
    }

    using var manifest =
      OpenJson("city-context.json");

    var root =
      manifest.RootElement;

    Assert.Equal(
      "demo-town",
      root.GetProperty("townId").GetString());
    Assert.Equal(
      "v1",
      root.GetProperty("version").GetString());
    Assert.Equal(
      "synthetic",
      root.GetProperty("dataClassification").GetString());
    Assert.True(
      root.GetProperty("redistributable").GetBoolean());
  }

  [Fact]
  public void Demo_town_geojson_has_one_boundary_zones_pois_and_a_pedestrian_network()
  {
    var boundary =
      ReadFeatures(
        "geography/boundary.geojson",
        "Polygon");
    var zones =
      ReadFeatures(
        "geography/zones.geojson",
        "Polygon");
    var pointsOfInterest =
      ReadFeatures(
        "geography/pois.geojson",
        "Point");
    var pedestrianLinks =
      ReadFeatures(
        "networks/pedestrian.geojson",
        "LineString");

    Assert.Single(boundary);
    Assert.Equal(
      "demo-town-boundary",
      boundary[0]
        .GetProperty("properties")
        .GetProperty("boundaryId")
        .GetString());

    Assert.Equal(
      3,
      zones.Count);
    Assert.Equal(
      4,
      pointsOfInterest.Count);
    Assert.True(
      pedestrianLinks.Count >= 4);

    var zoneIds =
      zones
        .Select(
          feature =>
            RequiredString(
              feature.GetProperty("properties"),
              "zoneId"))
        .ToHashSet(
          StringComparer.Ordinal);

    Assert.Equal(
      3,
      zoneIds.Count);

    var poiIds =
      pointsOfInterest
        .Select(
          feature =>
            RequiredString(
              feature.GetProperty("properties"),
              "pointOfInterestId"))
        .ToHashSet(
          StringComparer.Ordinal);

    Assert.Equal(
      4,
      poiIds.Count);
  }

  [Fact]
  public void Demo_town_city_context_maps_to_the_v1_3_domain_contracts()
  {
    using var manifest =
      OpenJson("city-context.json");

    var root =
      manifest.RootElement;
    var crs =
      ReadCrs(
        root.GetProperty("crs"));
    var provenance =
      ReadProvenance(
        root.GetProperty("provenance"));

    var categories =
      root
        .GetProperty("destinationCategories")
        .EnumerateArray()
        .Select(
          item =>
            DestinationCategory.Create(
              RequiredString(
                item,
                "categoryId")))
        .ToArray();

    var zones =
      ReadFeatures(
          "geography/zones.geojson",
          "Polygon")
        .Select(
          feature =>
          {
            var zoneId =
              RequiredString(
                feature.GetProperty("properties"),
                "zoneId");

            return CityZone.Create(
              ZonePolygonReference.Create(
                zoneId,
                $"geography/zones.geojson#feature={zoneId}",
                crs),
              provenance);
          })
        .ToArray();

    var pointsOfInterest =
      ReadFeatures(
          "geography/pois.geojson",
          "Point")
        .Select(
          feature =>
          {
            var properties =
              feature.GetProperty("properties");
            var coordinates =
              feature
                .GetProperty("geometry")
                .GetProperty("coordinates");

            return PointOfInterest.Create(
              RequiredString(
                properties,
                "pointOfInterestId"),
              RequiredString(
                properties,
                "categoryId"),
              GeoPoint.Create(
                coordinates[0].GetDouble(),
                coordinates[1].GetDouble(),
                crs),
              provenance);
          })
        .ToArray();

    var networks =
      root
        .GetProperty("networks")
        .EnumerateArray()
        .Select(
          item =>
            MobilityNetworkReference.Create(
              RequiredString(
                item,
                "networkId"),
              Enum.Parse<MobilityNetworkKind>(
                RequiredString(
                  item,
                  "kind"),
                ignoreCase: false),
              RequiredString(
                item,
                "reference"),
              provenance))
        .ToArray();

    var transitNetwork =
      Assert.Single(
        networks,
        network =>
          network.Kind
          == MobilityNetworkKind.PublicTransport);

    var stopRows =
      ReadCsv(
        transitNetwork.NetworkReference
          + "/stops.txt");

    var transitStops =
      stopRows
        .Select(
          row =>
            TransitStopReference.Create(
              row["stop_id"],
              transitNetwork.NetworkId,
              $"{transitNetwork.NetworkReference}/stops.txt#stop_id={row["stop_id"]}",
              GeoPoint.Create(
                double.Parse(
                  row["stop_lon"],
                  System.Globalization.CultureInfo.InvariantCulture),
                double.Parse(
                  row["stop_lat"],
                  System.Globalization.CultureInfo.InvariantCulture),
                crs),
              provenance))
        .ToArray();

    var snapshot =
      CityContextSnapshot.Create(
        categories,
        zones,
        pointsOfInterest,
        networks,
        transitStops);

    Assert.Equal(
      3,
      snapshot.DestinationCategories.Count);
    Assert.Equal(
      3,
      snapshot.Zones.Count);
    Assert.Equal(
      4,
      snapshot.PointsOfInterest.Count);
    Assert.Equal(
      2,
      snapshot.Networks.Count);
    Assert.Equal(
      3,
      snapshot.TransitStops.Count);

    Assert.Contains(
      snapshot.DestinationCategories,
      category =>
        category.CategoryId == "healthcare");
    Assert.Contains(
      snapshot.DestinationCategories,
      category =>
        category.CategoryId == "education");
    Assert.Contains(
      snapshot.DestinationCategories,
      category =>
        category.CategoryId == "public-services");

    Assert.Contains(
      snapshot.Networks,
      network =>
        network.Kind
        == MobilityNetworkKind.Pedestrian);
    Assert.Contains(
      snapshot.Networks,
      network =>
        network.Kind
        == MobilityNetworkKind.PublicTransport);
  }

  [Fact]
  public void Demo_town_gtfs_is_a_small_internally_consistent_schedule_fixture()
  {
    var agencies =
      ReadCsv(
        "transit/gtfs/agency.txt");
    var stops =
      ReadCsv(
        "transit/gtfs/stops.txt");
    var routes =
      ReadCsv(
        "transit/gtfs/routes.txt");
    var trips =
      ReadCsv(
        "transit/gtfs/trips.txt");
    var stopTimes =
      ReadCsv(
        "transit/gtfs/stop_times.txt");
    var calendars =
      ReadCsv(
        "transit/gtfs/calendar.txt");
    var shapes =
      ReadCsv(
        "transit/gtfs/shapes.txt");
    var feedInfo =
      ReadCsv(
        "transit/gtfs/feed_info.txt");

    var agencyIds =
      agencies
        .Select(
          row => row["agency_id"])
        .ToHashSet(
          StringComparer.Ordinal);
    var stopIds =
      stops
        .Select(
          row => row["stop_id"])
        .ToHashSet(
          StringComparer.Ordinal);
    var routeIds =
      routes
        .Select(
          row => row["route_id"])
        .ToHashSet(
          StringComparer.Ordinal);
    var serviceIds =
      calendars
        .Select(
          row => row["service_id"])
        .ToHashSet(
          StringComparer.Ordinal);
    var shapeIds =
      shapes
        .Select(
          row => row["shape_id"])
        .ToHashSet(
          StringComparer.Ordinal);
    var tripIds =
      trips
        .Select(
          row => row["trip_id"])
        .ToHashSet(
          StringComparer.Ordinal);

    Assert.Single(
      agencies);
    Assert.Equal(
      3,
      stops.Count);
    Assert.Single(
      routes);
    Assert.Equal(
      2,
      trips.Count);
    Assert.Single(
      calendars);
    Assert.Single(
      feedInfo);

    foreach (var route in routes)
    {
      Assert.Contains(
        route["agency_id"],
        agencyIds);
    }

    foreach (var trip in trips)
    {
      Assert.Contains(
        trip["route_id"],
        routeIds);
      Assert.Contains(
        trip["service_id"],
        serviceIds);
      Assert.Contains(
        trip["shape_id"],
        shapeIds);

      var tripStopTimes =
        stopTimes
          .Where(
            row =>
              row["trip_id"]
              == trip["trip_id"])
          .OrderBy(
            row =>
              int.Parse(
                row["stop_sequence"],
                System.Globalization.CultureInfo.InvariantCulture))
          .ToArray();

      Assert.True(
        tripStopTimes.Length >= 2);

      foreach (var stopTime in tripStopTimes)
      {
        Assert.Contains(
          stopTime["stop_id"],
          stopIds);
      }
    }

    Assert.All(
      stopTimes,
      row =>
        Assert.Contains(
          row["trip_id"],
          tripIds));

    using var manifest =
      OpenJson("city-context.json");

    Assert.Equal(
      manifest.RootElement
        .GetProperty("version")
        .GetString(),
      feedInfo[0]["feed_version"]);
  }

  private static CoordinateReferenceSystem ReadCrs(
    JsonElement element) =>
    CoordinateReferenceSystem.Create(
      RequiredString(
        element,
        "authority"),
      RequiredString(
        element,
        "code"));

  private static CityContextProvenance ReadProvenance(
    JsonElement element) =>
    CityContextProvenance.Create(
      RequiredString(
        element,
        "sourceSystem"),
      RequiredString(
        element,
        "sourceReference"),
      OptionalString(
        element,
        "sourceVersion"),
      OptionalDateTimeOffset(
        element,
        "effectiveAt"),
      DateTimeOffset.Parse(
        RequiredString(
          element,
          "retrievedAt"),
        System.Globalization.CultureInfo.InvariantCulture));

  private static JsonDocument OpenJson(
    string relativePath) =>
    JsonDocument.Parse(
      File.ReadAllText(
        Path.Combine(
          SampleRoot,
          relativePath)));

  private static IReadOnlyList<JsonElement> ReadFeatures(
    string relativePath,
    string expectedGeometryType)
  {
    using var document =
      OpenJson(
        relativePath);

    Assert.Equal(
      "FeatureCollection",
      RequiredString(
        document.RootElement,
        "type"));

    var features =
      document.RootElement
        .GetProperty("features")
        .EnumerateArray()
        .Select(
          feature =>
            feature.Clone())
        .ToArray();

    Assert.NotEmpty(
      features);

    foreach (var feature in features)
    {
      Assert.Equal(
        "Feature",
        RequiredString(
          feature,
          "type"));
      Assert.Equal(
        expectedGeometryType,
        RequiredString(
          feature.GetProperty("geometry"),
          "type"));
    }

    return features;
  }

  private static IReadOnlyList<Dictionary<string, string>> ReadCsv(
    string relativePath)
  {
    var path =
      Path.Combine(
        SampleRoot,
        relativePath);

    var lines =
      File.ReadAllLines(path);

    Assert.True(
      lines.Length >= 2,
      $"CSV fixture '{relativePath}' must contain a header and at least one row.");

    var headers =
      lines[0].Split(',');

    return lines
      .Skip(1)
      .Where(
        line =>
          !string.IsNullOrWhiteSpace(line))
      .Select(
        line =>
        {
          var values =
            line.Split(',');

          Assert.Equal(
            headers.Length,
            values.Length);

          return headers
            .Zip(
              values,
              static (header, value) =>
                KeyValuePair.Create(
                  header,
                  value))
            .ToDictionary(
              static pair => pair.Key,
              static pair => pair.Value,
              StringComparer.Ordinal);
        })
      .ToArray();
  }

  private static string RequiredString(
    JsonElement element,
    string propertyName)
  {
    var value =
      element
        .GetProperty(propertyName)
        .GetString();

    Assert.False(
      string.IsNullOrWhiteSpace(value),
      $"'{propertyName}' must be a non-empty string.");

    return value;
  }

  private static string? OptionalString(
    JsonElement element,
    string propertyName)
  {
    if (!element.TryGetProperty(
          propertyName,
          out var value)
        || value.ValueKind
        == JsonValueKind.Null)
    {
      return null;
    }

    return value.GetString();
  }

  private static DateTimeOffset? OptionalDateTimeOffset(
    JsonElement element,
    string propertyName)
  {
    var value =
      OptionalString(
        element,
        propertyName);

    return value is null
      ? null
      : DateTimeOffset.Parse(
          value,
          System.Globalization.CultureInfo.InvariantCulture);
  }
}

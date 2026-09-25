using SmartCities.Geospatial;
using Xunit;

namespace SmartCities.Core.Tests.Geospatial;

public sealed class GeospatialPrimitivesTests
{
  [Fact]
  public void Coordinate_reference_system_preserves_canonical_authority_and_code()
  {
    var crs = CoordinateReferenceSystem.Create(
      " epsg ",
      "4326");

    Assert.Equal("EPSG", crs.Authority);
    Assert.Equal("4326", crs.Code);
    Assert.Equal("EPSG:4326", crs.Identifier);
    Assert.Equal(CoordinateReferenceSystem.Wgs84, crs);
  }

  [Fact]
  public void Coordinate_reference_system_rejects_missing_components()
  {
    Assert.Throws<ArgumentException>(
      () => CoordinateReferenceSystem.Create(
        " ",
        "4326"));
    Assert.Throws<ArgumentException>(
      () => CoordinateReferenceSystem.Create(
        "EPSG",
        " "));
  }

  [Fact]
  public void Epsg_factory_rejects_non_positive_codes()
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => CoordinateReferenceSystem.Epsg(0));
    Assert.Throws<ArgumentOutOfRangeException>(
      () => CoordinateReferenceSystem.Epsg(-1));
  }

  [Fact]
  public void Point_uses_xy_order_and_validates_wgs84_longitude_latitude_ranges()
  {
    var point = GeoPoint.Create(
      x: -102.2916,
      y: 21.8818,
      CoordinateReferenceSystem.Wgs84);

    Assert.Equal(-102.2916, point.X);
    Assert.Equal(21.8818, point.Y);
    Assert.Equal(CoordinateReferenceSystem.Wgs84, point.Crs);

    Assert.Throws<ArgumentOutOfRangeException>(
      () => GeoPoint.Create(
        180.000001,
        0,
        CoordinateReferenceSystem.Wgs84));
    Assert.Throws<ArgumentOutOfRangeException>(
      () => GeoPoint.Create(
        0,
        90.000001,
        CoordinateReferenceSystem.Wgs84));
  }

  [Theory]
  [InlineData(double.NaN, 0)]
  [InlineData(double.PositiveInfinity, 0)]
  [InlineData(0, double.NegativeInfinity)]
  public void Point_rejects_non_finite_coordinates(
    double x,
    double y)
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => GeoPoint.Create(
        x,
        y,
        CoordinateReferenceSystem.Create(
          "LOCAL",
          "demo-grid")));
  }

  [Fact]
  public void Point_allows_finite_provider_neutral_coordinates_for_other_crs()
  {
    var point = GeoPoint.Create(
      500_000,
      2_420_000,
      CoordinateReferenceSystem.Epsg(32613));

    Assert.Equal(500_000, point.X);
    Assert.Equal(2_420_000, point.Y);
    Assert.Equal("EPSG:32613", point.Crs.Identifier);
  }

  [Fact]
  public void Bounding_box_requires_ordered_extents_and_one_crs()
  {
    var bbox = GeoBoundingBox.Create(
      minX: -102.5,
      minY: 21.7,
      maxX: -102.1,
      maxY: 22.0,
      CoordinateReferenceSystem.Wgs84);

    Assert.Equal(-102.5, bbox.MinX);
    Assert.Equal(21.7, bbox.MinY);
    Assert.Equal(-102.1, bbox.MaxX);
    Assert.Equal(22.0, bbox.MaxY);
    Assert.Equal(CoordinateReferenceSystem.Wgs84, bbox.Crs);

    Assert.Throws<ArgumentException>(
      () => GeoBoundingBox.Create(
        10,
        0,
        -10,
        1,
        CoordinateReferenceSystem.Wgs84));
    Assert.Throws<ArgumentException>(
      () => GeoBoundingBox.Create(
        0,
        10,
        1,
        -10,
        CoordinateReferenceSystem.Wgs84));
  }

  [Fact]
  public void Bounding_box_reuses_wgs84_coordinate_validation()
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => GeoBoundingBox.Create(
        -181,
        20,
        -102,
        22,
        CoordinateReferenceSystem.Wgs84));
    Assert.Throws<ArgumentOutOfRangeException>(
      () => GeoBoundingBox.Create(
        -103,
        -91,
        -102,
        22,
        CoordinateReferenceSystem.Wgs84));
  }

  [Fact]
  public void Path_requires_at_least_two_points_in_the_same_crs_and_defensively_copies()
  {
    var points = new List<GeoPoint>
    {
      GeoPoint.Create(
        -102.30,
        21.88,
        CoordinateReferenceSystem.Wgs84),
      GeoPoint.Create(
        -102.29,
        21.89,
        CoordinateReferenceSystem.Wgs84),
    };

    var path = GeoPath.Create(points);

    points.Add(
      GeoPoint.Create(
        -102.28,
        21.90,
        CoordinateReferenceSystem.Wgs84));

    Assert.Equal(2, path.Points.Count);
    Assert.Equal(
      CoordinateReferenceSystem.Wgs84,
      path.Crs);

    Assert.Throws<ArgumentException>(
      () => GeoPath.Create(
        [
          GeoPoint.Create(
            0,
            0,
            CoordinateReferenceSystem.Wgs84),
        ]));

    Assert.Throws<ArgumentException>(
      () => GeoPath.Create(
        [
          GeoPoint.Create(
            0,
            0,
            CoordinateReferenceSystem.Wgs84),
          GeoPoint.Create(
            500_000,
            2_420_000,
            CoordinateReferenceSystem.Epsg(32613)),
        ]));
  }

  [Fact]
  public void Zone_polygon_reference_keeps_geometry_external_and_crs_explicit()
  {
    var zone = ZonePolygonReference.Create(
      "zone-centro",
      "demo-town:polygons:zone-centro:v1",
      CoordinateReferenceSystem.Wgs84);

    Assert.Equal("zone-centro", zone.ZoneId);
    Assert.Equal(
      "demo-town:polygons:zone-centro:v1",
      zone.PolygonReference);
    Assert.Equal(
      CoordinateReferenceSystem.Wgs84,
      zone.Crs);
  }

  [Fact]
  public void Zone_polygon_reference_rejects_missing_identifiers()
  {
    Assert.Throws<ArgumentException>(
      () => ZonePolygonReference.Create(
        " ",
        "polygon-1",
        CoordinateReferenceSystem.Wgs84));
    Assert.Throws<ArgumentException>(
      () => ZonePolygonReference.Create(
        "zone-1",
        " ",
        CoordinateReferenceSystem.Wgs84));
  }

  [Theory]
  [InlineData(DistanceSemantics.Geodesic)]
  [InlineData(DistanceSemantics.Planar)]
  [InlineData(DistanceSemantics.PathLength)]
  [InlineData(DistanceSemantics.Network)]
  public void Distance_uses_meters_as_canonical_unit_and_requires_semantics(
    DistanceSemantics semantics)
  {
    var distance = Distance.CreateMeters(
      1_250,
      semantics);

    Assert.Equal(1_250, distance.Meters);
    Assert.Equal(1.25, distance.Kilometers);
    Assert.Equal(semantics, distance.Semantics);
  }

  [Theory]
  [InlineData(-0.001)]
  [InlineData(double.NaN)]
  [InlineData(double.PositiveInfinity)]
  public void Distance_rejects_negative_or_non_finite_values(
    double meters)
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => Distance.CreateMeters(
        meters,
        DistanceSemantics.Network));
  }

  [Fact]
  public void Distance_rejects_undefined_semantics()
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => Distance.CreateMeters(
        10,
        (DistanceSemantics)999));
  }
}

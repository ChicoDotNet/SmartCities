using System.Globalization;

namespace SmartCities.Geospatial;

/// <summary>
/// Identifies the coordinate reference system used by a SmartCities geospatial contract.
/// </summary>
/// <remarks>
/// The authority/code pair is provider-neutral metadata. SmartCities does not embed a projection engine.
/// Adapters remain responsible for transforming coordinates between reference systems when required.
/// </remarks>
public sealed record CoordinateReferenceSystem
{
  private CoordinateReferenceSystem(
    string authority,
    string code)
  {
    Authority = authority;
    Code = code;
  }

  /// <summary>Gets the canonical CRS authority, such as <c>EPSG</c>.</summary>
  public string Authority { get; }

  /// <summary>Gets the authority-local CRS code.</summary>
  public string Code { get; }

  /// <summary>Gets the canonical <c>AUTHORITY:CODE</c> identifier.</summary>
  public string Identifier =>
    $"{Authority}:{Code}";

  /// <summary>Gets WGS 84 identified as <c>EPSG:4326</c>.</summary>
  public static CoordinateReferenceSystem Wgs84 { get; } =
    new(
      "EPSG",
      "4326");

  /// <summary>Creates a provider-neutral CRS identifier.</summary>
  public static CoordinateReferenceSystem Create(
    string authority,
    string code)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      authority);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      code);

    var normalizedAuthority =
      authority.Trim().ToUpperInvariant();
    var normalizedCode =
      code.Trim();

    if (string.Equals(
          normalizedAuthority,
          Wgs84.Authority,
          StringComparison.Ordinal)
        && string.Equals(
          normalizedCode,
          Wgs84.Code,
          StringComparison.Ordinal))
    {
      return Wgs84;
    }

    return new CoordinateReferenceSystem(
      normalizedAuthority,
      normalizedCode);
  }

  /// <summary>Creates an EPSG coordinate reference system identifier.</summary>
  public static CoordinateReferenceSystem Epsg(
    int code)
  {
    if (code <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(code),
        code,
        "EPSG codes must be positive.");
    }

    return Create(
      "EPSG",
      code.ToString(
        CultureInfo.InvariantCulture));
  }

  internal bool IsWgs84 =>
    string.Equals(
      Authority,
      Wgs84.Authority,
      StringComparison.Ordinal)
    && string.Equals(
      Code,
      Wgs84.Code,
      StringComparison.Ordinal);
}

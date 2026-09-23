namespace SmartCities.Evidence;

/// <summary>Classifies the public kind of an evidence reference without exposing provider-specific storage details.</summary>
public enum EvidenceKind
{
  /// <summary>A statement or observation supplied by a citizen.</summary>
  CitizenStatement = 0,

  /// <summary>A photographic evidence item.</summary>
  Photograph = 1,

  /// <summary>A geospatial location or location-derived observation.</summary>
  Location = 2,

  /// <summary>A document or document-derived reference.</summary>
  Document = 3,

  /// <summary>A dataset or structured-data reference.</summary>
  Dataset = 4,

  /// <summary>A sensor or telemetry observation.</summary>
  SensorObservation = 5,

  /// <summary>An evidence type that does not fit another public category.</summary>
  Other = 6,
}

namespace SmartCities.CityContext;

/// <summary>
/// References an externally owned mobility network without importing routing-engine or GIS-native types.
/// </summary>
public sealed record MobilityNetworkReference
{
  private MobilityNetworkReference(
    string networkId,
    MobilityNetworkKind kind,
    string networkReference,
    CityContextProvenance provenance)
  {
    NetworkId = networkId;
    Kind = kind;
    NetworkReference = networkReference;
    Provenance = provenance;
  }

  /// <summary>Gets the stable SmartCities network identifier.</summary>
  public string NetworkId { get; }

  /// <summary>Gets the minimal provider-neutral network kind.</summary>
  public MobilityNetworkKind Kind { get; }

  /// <summary>Gets the external/source-local reference used to resolve the network.</summary>
  public string NetworkReference { get; }

  /// <summary>Gets the source provenance for the network reference.</summary>
  public CityContextProvenance Provenance { get; }

  /// <summary>Creates a validated mobility-network reference.</summary>
  public static MobilityNetworkReference Create(
    string networkId,
    MobilityNetworkKind kind,
    string networkReference,
    CityContextProvenance provenance)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      networkId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      networkReference);
    ArgumentNullException.ThrowIfNull(
      provenance);

    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Mobility network kind must be a defined value.");
    }

    return new MobilityNetworkReference(
      networkId.Trim(),
      kind,
      networkReference.Trim(),
      provenance);
  }
}

namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Describes provider-neutral capabilities exposed by a routing engine.
/// </summary>
public sealed record AccessibilityRoutingCapabilities
{
  private AccessibilityRoutingCapabilities(
    string engineId,
    IReadOnlyList<JourneyMode> supportedModes,
    bool supportsScheduledTransit,
    bool supportsAccessibilityInformation,
    bool supportsPathGeometry)
  {
    EngineId = engineId;
    SupportedModes = supportedModes;
    SupportsScheduledTransit = supportsScheduledTransit;
    SupportsAccessibilityInformation = supportsAccessibilityInformation;
    SupportsPathGeometry = supportsPathGeometry;
  }

  /// <summary>Gets the stable engine identifier.</summary>
  public string EngineId { get; }

  /// <summary>Gets the normalized modes supported by this engine/adapter configuration.</summary>
  public IReadOnlyList<JourneyMode> SupportedModes { get; }

  /// <summary>Gets whether scheduled public-transport routing is supported.</summary>
  public bool SupportsScheduledTransit { get; }

  /// <summary>Gets whether the engine can return normalized accessibility information.</summary>
  public bool SupportsAccessibilityInformation { get; }

  /// <summary>Gets whether journey legs can include path geometry.</summary>
  public bool SupportsPathGeometry { get; }

  /// <summary>Creates validated routing capabilities.</summary>
  public static AccessibilityRoutingCapabilities Create(
    string engineId,
    IEnumerable<JourneyMode> supportedModes,
    bool supportsScheduledTransit,
    bool supportsAccessibilityInformation,
    bool supportsPathGeometry)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      engineId);
    ArgumentNullException.ThrowIfNull(
      supportedModes);

    var modes =
      supportedModes.ToArray();

    if (modes.Length == 0)
    {
      throw new ArgumentException(
        "At least one supported journey mode is required.",
        nameof(supportedModes));
    }

    foreach (var mode in modes)
    {
      if (!Enum.IsDefined(mode))
      {
        throw new ArgumentOutOfRangeException(
          nameof(supportedModes),
          mode,
          "Journey mode must be a defined value.");
      }
    }

    if (modes.Distinct().Count()
        != modes.Length)
    {
      throw new ArgumentException(
        "Supported journey modes must be unique.",
        nameof(supportedModes));
    }

    return new AccessibilityRoutingCapabilities(
      engineId.Trim(),
      Array.AsReadOnly(modes),
      supportsScheduledTransit,
      supportsAccessibilityInformation,
      supportsPathGeometry);
  }
}

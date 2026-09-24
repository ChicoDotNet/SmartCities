namespace SmartCities.Application.FeatureFlags;

/// <summary>
/// Identifies the Town Hall deployment whose non-sensitive configuration is being resolved.
/// </summary>
public sealed record TownHallContext
{
  /// <summary>Initializes a validated Town Hall identity.</summary>
  public TownHallContext(string townHallId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(townHallId);

    var normalized = townHallId.Trim();

    if (normalized.Length > 128
      || normalized.Any(
        static character =>
          !char.IsAsciiLetterOrDigit(character)
          && character is not '-' and not '_' and not '.'))
    {
      throw new ArgumentException(
        "Town Hall identifiers may contain only ASCII letters, digits, '-', '_' and '.', with a maximum length of 128.",
        nameof(townHallId));
    }

    TownHallId = normalized;
  }

  /// <summary>Gets the stable deployment identifier.</summary>
  public string TownHallId { get; }
}

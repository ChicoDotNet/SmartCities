using System.Collections.ObjectModel;

namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Preserves normalized accessibility state and non-localized limitation codes.
/// </summary>
public sealed record JourneyAccessibility
{
  private JourneyAccessibility(
    AccessibilityStatus status,
    IReadOnlyList<string> limitationCodes)
  {
    Status = status;
    LimitationCodes = limitationCodes;
  }

  /// <summary>Gets the normalized accessibility state.</summary>
  public AccessibilityStatus Status { get; }

  /// <summary>Gets stable, non-localized known limitation codes.</summary>
  public IReadOnlyList<string> LimitationCodes { get; }

  /// <summary>Creates accessibility information with explicit knowledge semantics.</summary>
  public static JourneyAccessibility Create(
    AccessibilityStatus status,
    IEnumerable<string> limitationCodes)
  {
    if (!Enum.IsDefined(status))
    {
      throw new ArgumentOutOfRangeException(
        nameof(status),
        status,
        "Accessibility status must be a defined value.");
    }

    ArgumentNullException.ThrowIfNull(
      limitationCodes);

    var normalized =
      limitationCodes
        .Select(
          code =>
          {
            ArgumentException.ThrowIfNullOrWhiteSpace(
              code);
            return code.Trim();
          })
        .ToArray();

    if (normalized.Distinct(
          StringComparer.Ordinal).Count()
        != normalized.Length)
    {
      throw new ArgumentException(
        "Accessibility limitation codes must be unique.",
        nameof(limitationCodes));
    }

    if ((status == AccessibilityStatus.Unknown
         || status == AccessibilityStatus.KnownAccessible)
        && normalized.Length != 0)
    {
      throw new ArgumentException(
        "Unknown or known-accessible state cannot carry limitation codes.",
        nameof(limitationCodes));
    }

    if (status == AccessibilityStatus.KnownLimited
        && normalized.Length == 0)
    {
      throw new ArgumentException(
        "Known-limited accessibility requires at least one limitation code.",
        nameof(limitationCodes));
    }

    return new JourneyAccessibility(
      status,
      Array.AsReadOnly(normalized));
  }

  /// <summary>Creates an explicit unknown accessibility state.</summary>
  public static JourneyAccessibility Unknown() =>
    Create(
      AccessibilityStatus.Unknown,
      []);

  /// <summary>Creates an explicit known-accessible state.</summary>
  public static JourneyAccessibility KnownAccessible() =>
    Create(
      AccessibilityStatus.KnownAccessible,
      []);

  /// <summary>Creates a known-limited state with one or more stable limitation codes.</summary>
  public static JourneyAccessibility KnownLimited(
    params string[] limitationCodes) =>
    Create(
      AccessibilityStatus.KnownLimited,
      limitationCodes);

  /// <summary>Creates a known-inaccessible state with optional stable limitation codes.</summary>
  public static JourneyAccessibility KnownInaccessible(
    params string[] limitationCodes) =>
    Create(
      AccessibilityStatus.KnownInaccessible,
      limitationCodes);
}

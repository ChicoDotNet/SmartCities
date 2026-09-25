namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Represents one expected provider-neutral routing failure.
/// </summary>
public sealed record AccessibilityRoutingFailure
{
  private AccessibilityRoutingFailure(
    AccessibilityRoutingFailureCode code)
  {
    Code = code;
  }

  /// <summary>Gets the provider-neutral failure category.</summary>
  public AccessibilityRoutingFailureCode Code { get; }

  /// <summary>Creates a validated routing failure.</summary>
  public static AccessibilityRoutingFailure Create(
    AccessibilityRoutingFailureCode code)
  {
    if (!Enum.IsDefined(code))
    {
      throw new ArgumentOutOfRangeException(
        nameof(code),
        code,
        "Routing failure code must be a defined value.");
    }

    return new AccessibilityRoutingFailure(
      code);
  }
}

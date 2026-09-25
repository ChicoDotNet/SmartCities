namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Defines caller execution policy that is separate from route semantics.
/// </summary>
public sealed record AccessibilityRoutingExecutionOptions
{
  private AccessibilityRoutingExecutionOptions(
    TimeSpan timeout)
  {
    Timeout = timeout;
  }

  /// <summary>Gets the maximum routing execution time requested by the caller.</summary>
  public TimeSpan Timeout { get; }

  /// <summary>Creates validated routing execution options.</summary>
  public static AccessibilityRoutingExecutionOptions Create(
    TimeSpan timeout)
  {
    if (timeout <= TimeSpan.Zero
        || timeout == Timeout.InfiniteTimeSpan)
    {
      throw new ArgumentOutOfRangeException(
        nameof(timeout),
        timeout,
        "Routing timeout must be finite and greater than zero.");
    }

    return new AccessibilityRoutingExecutionOptions(
      timeout);
  }
}

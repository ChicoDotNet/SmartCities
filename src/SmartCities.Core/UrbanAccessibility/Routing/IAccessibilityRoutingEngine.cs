namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Defines the provider-neutral routing boundary used by Urban Accessibility.
/// </summary>
/// <remarks>
/// Provider-specific requests, responses, authentication, health models, and errors remain behind concrete adapters.
/// Expected routing failures are returned through <see cref="AccessibilityRouteResult"/>.
/// Caller cancellation follows normal .NET cancellation semantics and must not be converted into a routing failure.
/// </remarks>
public interface IAccessibilityRoutingEngine
{
  /// <summary>Discovers the configured engine/adapter capabilities.</summary>
  Task<AccessibilityRoutingCapabilities> GetCapabilitiesAsync(
    CancellationToken cancellationToken = default);

  /// <summary>Routes one normalized point-to-point query within explicit execution policy.</summary>
  Task<AccessibilityRouteResult> RouteAsync(
    AccessibilityRouteQuery query,
    AccessibilityRoutingExecutionOptions options,
    CancellationToken cancellationToken = default);
}

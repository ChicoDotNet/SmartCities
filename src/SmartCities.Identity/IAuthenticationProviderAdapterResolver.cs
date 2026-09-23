namespace SmartCities.Identity;

/// <summary>
/// Resolves exactly one authentication-provider adapter for a validated provider context.
/// </summary>
public interface IAuthenticationProviderAdapterResolver
{
  /// <summary>Resolves the single adapter that explicitly accepts the context.</summary>
  /// <param name="context">Validated external provider context.</param>
  /// <returns>The matching provider adapter.</returns>
  /// <exception cref="InvalidOperationException">Thrown when zero or multiple adapters match.</exception>
  IAuthenticationProviderAdapter Resolve(
    AuthenticationProviderContext context);
}

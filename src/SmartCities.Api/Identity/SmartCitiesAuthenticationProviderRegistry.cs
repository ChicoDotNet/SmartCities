using System.Collections.ObjectModel;

namespace SmartCities.Api.Identity;

/// <summary>
/// Provides the immutable public registry of enabled SmartCities authentication providers.
/// </summary>
public sealed class SmartCitiesAuthenticationProviderRegistry
{
  internal SmartCitiesAuthenticationProviderRegistry(
    IEnumerable<SmartCitiesAuthenticationProviderDescriptor> providers)
  {
    ArgumentNullException.ThrowIfNull(providers);

    Providers =
      new ReadOnlyCollection<
        SmartCitiesAuthenticationProviderDescriptor>(
          providers
            .OrderBy(
              static item => item.ProviderId,
              StringComparer.Ordinal)
            .ToArray());
  }

  /// <summary>Gets all enabled providers without exposing credentials or signing material.</summary>
  public IReadOnlyList<SmartCitiesAuthenticationProviderDescriptor> Providers { get; }
}

namespace SmartCities.Identity;

/// <summary>
/// Resolves provider adapters with fail-closed unknown and ambiguous issuer semantics.
/// </summary>
public sealed class AuthenticationProviderAdapterResolver
  : IAuthenticationProviderAdapterResolver
{
  private readonly IReadOnlyList<IAuthenticationProviderAdapter> adapters;

  /// <summary>Initializes the resolver with all configured provider adapters.</summary>
  /// <param name="adapters">Configured provider adapters.</param>
  public AuthenticationProviderAdapterResolver(
    IEnumerable<IAuthenticationProviderAdapter> adapters)
  {
    ArgumentNullException.ThrowIfNull(adapters);

    var snapshot = adapters.ToArray();

    if (snapshot.Any(static adapter => adapter is null))
    {
      throw new ArgumentException(
        "Authentication provider adapters cannot contain null values.",
        nameof(adapters));
    }

    foreach (var adapter in snapshot)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(
        adapter.ProviderId);
    }

    if (snapshot
      .GroupBy(
        static adapter => adapter.ProviderId,
        StringComparer.Ordinal)
      .Any(static group => group.Count() > 1))
    {
      throw new ArgumentException(
        "Authentication provider identifiers must be unique.",
        nameof(adapters));
    }

    this.adapters =
      Array.AsReadOnly(snapshot);
  }

  /// <inheritdoc />
  public IAuthenticationProviderAdapter Resolve(
    AuthenticationProviderContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var matches = adapters
      .Where(
        adapter => adapter.CanHandle(context))
      .ToArray();

    return matches.Length switch
    {
      1 => matches[0],
      0 => throw new InvalidOperationException(
        $"No authentication provider adapter accepts issuer '{context.Issuer}'."),
      _ => throw new InvalidOperationException(
        $"Multiple authentication provider adapters accept issuer '{context.Issuer}'."),
    };
  }
}

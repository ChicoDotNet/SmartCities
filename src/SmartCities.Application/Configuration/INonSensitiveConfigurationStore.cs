namespace SmartCities.Application.Configuration;

/// <summary>
/// Stores deployment configuration values that are explicitly classified as non-sensitive.
/// </summary>
/// <remarks>
/// Implementations are not secret stores. Credentials, tokens, private keys, citizen evidence,
/// authentication material, or any other secret/sensitive value must not cross this boundary.
/// </remarks>
public interface INonSensitiveConfigurationStore
{
  /// <summary>Gets a non-sensitive setting for one Town Hall.</summary>
  Task<string?> GetAsync(
    string townHallId,
    string key,
    CancellationToken cancellationToken = default);

  /// <summary>Persists a non-sensitive setting for one Town Hall.</summary>
  Task SetAsync(
    string townHallId,
    string key,
    string value,
    CancellationToken cancellationToken = default);
}

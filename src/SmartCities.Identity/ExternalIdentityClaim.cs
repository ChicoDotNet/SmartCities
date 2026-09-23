namespace SmartCities.Identity;

/// <summary>
/// Represents one claim emitted by an already validated external authentication provider.
/// </summary>
public sealed record ExternalIdentityClaim
{
  private ExternalIdentityClaim(
    string type,
    string value)
  {
    Type = type;
    Value = value;
  }

  /// <summary>Gets the provider-native claim type.</summary>
  public string Type { get; }

  /// <summary>Gets the provider-native claim value.</summary>
  public string Value { get; }

  /// <summary>Creates a validated external claim value.</summary>
  /// <param name="type">Non-empty provider-native claim type.</param>
  /// <param name="value">Non-empty provider-native claim value.</param>
  /// <returns>An immutable external claim.</returns>
  public static ExternalIdentityClaim Create(
    string type,
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(type);
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    return new ExternalIdentityClaim(
      type.Trim(),
      value.Trim());
  }
}

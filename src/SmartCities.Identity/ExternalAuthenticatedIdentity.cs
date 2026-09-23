namespace SmartCities.Identity;

/// <summary>
/// Represents the provider-native identity after credential/protocol validation but before SmartCities canonicalization.
/// </summary>
public sealed record ExternalAuthenticatedIdentity
{
  private ExternalAuthenticatedIdentity(
    string subject,
    IReadOnlyList<ExternalIdentityClaim> claims)
  {
    Subject = subject;
    Claims = claims;
  }

  /// <summary>Gets the validated provider-native subject identifier.</summary>
  public string Subject { get; }

  /// <summary>Gets the provider-native claims available to the adapter.</summary>
  public IReadOnlyList<ExternalIdentityClaim> Claims { get; }

  /// <summary>Creates a snapshot of an already validated external identity.</summary>
  /// <param name="subject">Validated provider-native subject identifier.</param>
  /// <param name="claims">Provider-native claims to be interpreted only by the matching adapter.</param>
  /// <returns>An immutable external-identity snapshot.</returns>
  public static ExternalAuthenticatedIdentity Create(
    string subject,
    IEnumerable<ExternalIdentityClaim> claims)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(subject);
    ArgumentNullException.ThrowIfNull(claims);

    var claimSnapshot = claims.ToArray();

    if (claimSnapshot.Any(static claim => claim is null))
    {
      throw new ArgumentException(
        "External identity claims cannot contain null values.",
        nameof(claims));
    }

    return new ExternalAuthenticatedIdentity(
      subject.Trim(),
      Array.AsReadOnly(claimSnapshot));
  }
}

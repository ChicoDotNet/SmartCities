namespace SmartCities.Identity;

/// <summary>
/// Represents provider-neutral identity and authorization data after trusted provider canonicalization.
/// </summary>
public sealed record CanonicalIdentity
{
  private CanonicalIdentity(
    string identityProvider,
    string subjectId,
    IReadOnlyList<string> authorityRoles,
    IReadOnlyList<string> permissions)
  {
    IdentityProvider = identityProvider;
    SubjectId = subjectId;
    AuthorityRoles = authorityRoles;
    Permissions = permissions;
  }

  /// <summary>Gets the stable SmartCities authentication-provider identifier.</summary>
  public string IdentityProvider { get; }

  /// <summary>Gets the globally stable SmartCities subject identifier.</summary>
  public string SubjectId { get; }

  /// <summary>Gets the canonical authority roles associated with the identity.</summary>
  public IReadOnlyList<string> AuthorityRoles { get; }

  /// <summary>Gets the canonical SmartCities permissions associated with the identity.</summary>
  public IReadOnlyList<string> Permissions { get; }

  /// <summary>Creates a validated canonical identity.</summary>
  /// <param name="identityProvider">Stable SmartCities provider identifier.</param>
  /// <param name="subjectId">Globally stable SmartCities subject identifier.</param>
  /// <param name="authorityRoles">Canonical authority-role values.</param>
  /// <param name="permissions">Canonical SmartCities permission values.</param>
  /// <returns>An immutable canonical identity.</returns>
  public static CanonicalIdentity Create(
    string identityProvider,
    string subjectId,
    IEnumerable<string> authorityRoles,
    IEnumerable<string> permissions)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      identityProvider);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      subjectId);
    ArgumentNullException.ThrowIfNull(
      authorityRoles);
    ArgumentNullException.ThrowIfNull(
      permissions);

    return new CanonicalIdentity(
      identityProvider.Trim(),
      subjectId.Trim(),
      ValidateAuthorizationValues(
        authorityRoles,
        nameof(authorityRoles)),
      ValidateAuthorizationValues(
        permissions,
        nameof(permissions)));
  }

  private static IReadOnlyList<string> ValidateAuthorizationValues(
    IEnumerable<string> values,
    string parameterName)
  {
    var snapshot = values
      .Select(
        value =>
        {
          if (string.IsNullOrWhiteSpace(value))
          {
            throw new ArgumentException(
              "Canonical authorization values cannot be empty or whitespace.",
              parameterName);
          }

          return value.Trim();
        })
      .ToArray();

    if (snapshot.Distinct(StringComparer.Ordinal).Count()
      != snapshot.Length)
    {
      throw new ArgumentException(
        "Canonical authorization values must be unique.",
        parameterName);
    }

    return Array.AsReadOnly(snapshot);
  }
}

namespace SmartCities.Application.Administration;

/// <summary>
/// Contains the persisted canonical authorization material effective for one admitted identity.
/// </summary>
public sealed record AdministrationEffectiveGrants
{
  private AdministrationEffectiveGrants(
    IReadOnlyList<string> authorityRoles,
    IReadOnlyList<string> permissions)
  {
    AuthorityRoles = authorityRoles;
    Permissions = permissions;
  }

  /// <summary>Gets the persisted canonical authority roles.</summary>
  public IReadOnlyList<string> AuthorityRoles { get; }

  /// <summary>Gets the persisted canonical permissions.</summary>
  public IReadOnlyList<string> Permissions { get; }

  /// <summary>Gets an empty persisted-grants result.</summary>
  public static AdministrationEffectiveGrants Empty { get; } =
    new([], []);

  /// <summary>Creates a normalized persisted-grants result.</summary>
  public static AdministrationEffectiveGrants Create(
    IEnumerable<string> authorityRoles,
    IEnumerable<string> permissions)
  {
    ArgumentNullException.ThrowIfNull(authorityRoles);
    ArgumentNullException.ThrowIfNull(permissions);

    return new AdministrationEffectiveGrants(
      authorityRoles
        .Where(static value => !string.IsNullOrWhiteSpace(value))
        .Select(static value => value.Trim())
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray(),
      permissions
        .Where(static value => !string.IsNullOrWhiteSpace(value))
        .Select(static value => value.Trim())
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray());
  }
}

namespace SmartCities.Application.Administration;

/// <summary>
/// Represents one persisted per-Town-Hall authorization grant targeted by a portable identity selector.
/// </summary>
public sealed record AdministrationAuthorizationGrant
{
  private AdministrationAuthorizationGrant(
    string grantId,
    AdministrationAccessRuleKind targetKind,
    string targetValue,
    AdministrationAuthorizationGrantKind kind,
    string value)
  {
    GrantId = grantId;
    TargetKind = targetKind;
    TargetValue = targetValue;
    Kind = kind;
    Value = value;
  }

  /// <summary>Gets the stable grant identifier.</summary>
  public string GrantId { get; }

  /// <summary>Gets the portable selector kind used to target identities.</summary>
  public AdministrationAccessRuleKind TargetKind { get; }

  /// <summary>Gets the normalized selector value.</summary>
  public string TargetValue { get; }

  /// <summary>Gets whether the grant assigns a role or a permission.</summary>
  public AdministrationAuthorizationGrantKind Kind { get; }

  /// <summary>Gets the canonical role/permission value.</summary>
  public string Value { get; }

  /// <summary>Creates and normalizes one persisted authorization grant.</summary>
  public static AdministrationAuthorizationGrant Create(
    string grantId,
    AdministrationAccessRuleKind targetKind,
    string targetValue,
    AdministrationAuthorizationGrantKind kind,
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(grantId);
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported Administration authorization grant kind.");
    }

    var normalizedId = grantId.Trim();

    if (normalizedId.Length > 128)
    {
      throw new ArgumentException(
        "Administration authorization grant identifiers cannot exceed 128 characters.",
        nameof(grantId));
    }

    var normalizedTarget = AdministrationAccessRule.Create(
      "grant-target",
      targetKind,
      targetValue);

    var normalizedValue = value.Trim();

    if (normalizedValue.Length is < 1 or > 256
      || normalizedValue.Any(
        static character =>
          char.IsControl(character)))
    {
      throw new ArgumentException(
        "Administration authorization grant values must be non-empty, bounded canonical values.",
        nameof(value));
    }

    return new AdministrationAuthorizationGrant(
      normalizedId,
      normalizedTarget.Kind,
      normalizedTarget.Value,
      kind,
      normalizedValue);
  }
}

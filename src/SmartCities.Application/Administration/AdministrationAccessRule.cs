using System.Net.Mail;

namespace SmartCities.Application.Administration;

/// <summary>
/// Represents one normalized per-Town-Hall administration admission rule.
/// </summary>
public sealed record AdministrationAccessRule
{
  private AdministrationAccessRule(
    string ruleId,
    AdministrationAccessRuleKind kind,
    string value)
  {
    RuleId = ruleId;
    Kind = kind;
    Value = value;
  }

  /// <summary>Gets the stable rule identifier.</summary>
  public string RuleId { get; }

  /// <summary>Gets the portable rule kind.</summary>
  public AdministrationAccessRuleKind Kind { get; }

  /// <summary>Gets the normalized match value.</summary>
  public string Value { get; }

  /// <summary>Creates and validates one whitelist rule.</summary>
  public static AdministrationAccessRule Create(
    string ruleId,
    AdministrationAccessRuleKind kind,
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported administration access rule kind.");
    }

    var normalizedRuleId = ruleId.Trim();

    if (normalizedRuleId.Length > 128)
    {
      throw new ArgumentException(
        "Administration access rule identifiers cannot exceed 128 characters.",
        nameof(ruleId));
    }

    return new AdministrationAccessRule(
      normalizedRuleId,
      kind,
      NormalizeValue(kind, value));
  }

  internal static string NormalizeEmail(
    string value)
  {
    if (!MailAddress.TryCreate(
        value.Trim(),
        out var address)
      || !string.Equals(
        address.Address,
        value.Trim(),
        StringComparison.OrdinalIgnoreCase))
    {
      throw new ArgumentException(
        "Administration email rules require one complete email address.",
        nameof(value));
    }

    return address.Address.ToLowerInvariant();
  }

  internal static string NormalizeDomain(
    string value)
  {
    var normalized = value.Trim();

    if (normalized.StartsWith(
        '@'))
    {
      normalized = normalized[1..];
    }

    normalized = normalized.ToLowerInvariant();

    if (normalized.Length is < 3 or > 253
      || normalized.Contains('*')
      || normalized.Contains('/')
      || normalized.Contains('@')
      || normalized.StartsWith('.')
      || normalized.EndsWith('.')
      || !normalized.Contains('.')
      || normalized.Split('.').Any(
        static label =>
          label.Length is < 1 or > 63
          || label.StartsWith('-')
          || label.EndsWith('-')
          || label.Any(
            static character =>
              !char.IsAsciiLetterOrDigit(character)
              && character != '-')))
    {
      throw new ArgumentException(
        "Administration domain rules require one exact DNS-style domain without wildcards.",
        nameof(value));
    }

    return normalized;
  }

  private static string NormalizeValue(
    AdministrationAccessRuleKind kind,
    string value) =>
    kind switch
    {
      AdministrationAccessRuleKind.Email =>
        NormalizeEmail(value),
      AdministrationAccessRuleKind.EmailDomain =>
        NormalizeDomain(value),
      AdministrationAccessRuleKind.CanonicalSubject =>
        NormalizeSubject(value),
      _ => throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported administration access rule kind."),
    };

  private static string NormalizeSubject(
    string value)
  {
    var normalized = value.Trim();

    if (normalized.Length is < 1 or > 512
      || normalized.Any(
        static character =>
          char.IsControl(character)))
    {
      throw new ArgumentException(
        "Administration canonical-subject rules require one non-empty canonical subject.",
        nameof(value));
    }

    return normalized;
  }
}

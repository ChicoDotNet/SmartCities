namespace SmartCities.Application.Administration;

/// <summary>
/// Contains only canonical identity material needed by the administration admission boundary.
/// </summary>
public sealed record AdministrationIdentity
{
  private AdministrationIdentity(
    string subjectId,
    string? emailAddress)
  {
    SubjectId = subjectId;
    EmailAddress = emailAddress;
  }

  /// <summary>Gets the canonical SmartCities subject.</summary>
  public string SubjectId { get; }

  /// <summary>Gets the optional canonical normalized email address.</summary>
  public string? EmailAddress { get; }

  /// <summary>Creates a validated administration identity snapshot.</summary>
  public static AdministrationIdentity Create(
    string subjectId,
    string? emailAddress)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);

    var normalizedSubject = subjectId.Trim();

    if (normalizedSubject.Length > 512
      || normalizedSubject.Any(
        static character =>
          char.IsControl(character)))
    {
      throw new ArgumentException(
        "Canonical administration subjects cannot exceed 512 characters or contain control characters.",
        nameof(subjectId));
    }

    return new AdministrationIdentity(
      normalizedSubject,
      string.IsNullOrWhiteSpace(emailAddress)
        ? null
        : AdministrationAccessRule.NormalizeEmail(
          emailAddress));
  }
}

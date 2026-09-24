namespace SmartCities.Application.Administration;

/// <summary>
/// Carries the canonical actor and request-correlation material required to audit one control-plane mutation.
/// </summary>
public sealed record AdministrationControlPlaneAuditContext
{
  private AdministrationControlPlaneAuditContext(
    string actorSubjectId,
    string actorIdentityProvider,
    string correlationId)
  {
    ActorSubjectId = actorSubjectId;
    ActorIdentityProvider = actorIdentityProvider;
    CorrelationId = correlationId;
  }

  /// <summary>Gets the canonical SmartCities subject executing the mutation.</summary>
  public string ActorSubjectId { get; }

  /// <summary>Gets the canonical identity provider that authenticated the actor.</summary>
  public string ActorIdentityProvider { get; }

  /// <summary>Gets the privacy-safe request correlation identifier.</summary>
  public string CorrelationId { get; }

  /// <summary>Creates validated canonical audit context.</summary>
  public static AdministrationControlPlaneAuditContext Create(
    string actorSubjectId,
    string actorIdentityProvider,
    string correlationId)
  {
    return new AdministrationControlPlaneAuditContext(
      Validate(
        actorSubjectId,
        nameof(actorSubjectId),
        512),
      Validate(
        actorIdentityProvider,
        nameof(actorIdentityProvider),
        128),
      Validate(
        correlationId,
        nameof(correlationId),
        64));
  }

  private static string Validate(
    string value,
    string parameterName,
    int maximumLength)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    var normalized = value.Trim();

    if (normalized.Length > maximumLength
      || normalized.Any(
        static character => char.IsControl(character)))
    {
      throw new ArgumentException(
        "Audit context values must be bounded printable strings.",
        parameterName);
    }

    return normalized;
  }
}

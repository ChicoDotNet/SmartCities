namespace SmartCities.Application.Administration;

/// <summary>
/// Represents one immutable completed Town Hall control-plane mutation.
/// </summary>
public sealed record AdministrationControlPlaneAuditEvent
{
  private AdministrationControlPlaneAuditEvent(
    string eventId,
    string townHallId,
    DateTimeOffset occurredAtUtc,
    string actorSubjectId,
    string actorIdentityProvider,
    string action,
    string resourceType,
    string resourceId,
    string descriptor,
    string? previousValue,
    string? newValue,
    string correlationId)
  {
    EventId = eventId;
    TownHallId = townHallId;
    OccurredAtUtc = occurredAtUtc;
    ActorSubjectId = actorSubjectId;
    ActorIdentityProvider = actorIdentityProvider;
    Action = action;
    ResourceType = resourceType;
    ResourceId = resourceId;
    Descriptor = descriptor;
    PreviousValue = previousValue;
    NewValue = newValue;
    CorrelationId = correlationId;
  }

  /// <summary>Gets the immutable event identifier.</summary>
  public string EventId { get; }

  /// <summary>Gets the Town Hall partition.</summary>
  public string TownHallId { get; }

  /// <summary>Gets when the completed mutation was recorded in UTC.</summary>
  public DateTimeOffset OccurredAtUtc { get; }

  /// <summary>Gets the canonical actor subject.</summary>
  public string ActorSubjectId { get; }

  /// <summary>Gets the canonical actor identity provider.</summary>
  public string ActorIdentityProvider { get; }

  /// <summary>Gets the stable machine action.</summary>
  public string Action { get; }

  /// <summary>Gets the stable resource type.</summary>
  public string ResourceType { get; }

  /// <summary>Gets the stable resource identifier.</summary>
  public string ResourceId { get; }

  /// <summary>Gets a bounded operator-facing resource descriptor.</summary>
  public string Descriptor { get; }

  /// <summary>Gets the previous bounded machine value, when applicable.</summary>
  public string? PreviousValue { get; }

  /// <summary>Gets the new bounded machine value, when applicable.</summary>
  public string? NewValue { get; }

  /// <summary>Gets the request correlation identifier.</summary>
  public string CorrelationId { get; }

  /// <summary>Creates one validated immutable audit event.</summary>
  public static AdministrationControlPlaneAuditEvent Create(
    string eventId,
    string townHallId,
    DateTimeOffset occurredAtUtc,
    string actorSubjectId,
    string actorIdentityProvider,
    string action,
    string resourceType,
    string resourceId,
    string descriptor,
    string? previousValue,
    string? newValue,
    string correlationId)
  {
    return new AdministrationControlPlaneAuditEvent(
      Required(eventId, nameof(eventId), 128),
      Required(townHallId, nameof(townHallId), 128),
      occurredAtUtc.ToUniversalTime(),
      Required(actorSubjectId, nameof(actorSubjectId), 512),
      Required(actorIdentityProvider, nameof(actorIdentityProvider), 128),
      Required(action, nameof(action), 128),
      Required(resourceType, nameof(resourceType), 128),
      Required(resourceId, nameof(resourceId), 512),
      Required(descriptor, nameof(descriptor), 1024),
      Optional(previousValue, nameof(previousValue), 512),
      Optional(newValue, nameof(newValue), 512),
      Required(correlationId, nameof(correlationId), 64));
  }

  /// <summary>Creates a completed mutation event from canonical request audit context.</summary>
  public static AdministrationControlPlaneAuditEvent CreateMutation(
    string townHallId,
    AdministrationControlPlaneAuditContext context,
    string action,
    string resourceType,
    string resourceId,
    string descriptor,
    string? previousValue = null,
    string? newValue = null)
  {
    ArgumentNullException.ThrowIfNull(context);

    return Create(
      Guid.NewGuid().ToString("N"),
      townHallId,
      DateTimeOffset.UtcNow,
      context.ActorSubjectId,
      context.ActorIdentityProvider,
      action,
      resourceType,
      resourceId,
      descriptor,
      previousValue,
      newValue,
      context.CorrelationId);
  }

  private static string Required(
    string value,
    string parameterName,
    int maximumLength)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    return Validate(
      value.Trim(),
      parameterName,
      maximumLength);
  }

  private static string? Optional(
    string? value,
    string parameterName,
    int maximumLength)
  {
    if (value is null)
    {
      return null;
    }

    return Validate(
      value.Trim(),
      parameterName,
      maximumLength);
  }

  private static string Validate(
    string value,
    string parameterName,
    int maximumLength)
  {
    if (value.Length > maximumLength
      || value.Any(
        static character => char.IsControl(character)))
    {
      throw new ArgumentException(
        "Audit values must be bounded printable strings.",
        parameterName);
    }

    return value;
  }
}

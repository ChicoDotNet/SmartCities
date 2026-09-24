namespace SmartCities.Api.Administration;

/// <summary>
/// Represents one immutable Town Hall control-plane audit entry.
/// </summary>
public sealed record AdministrationControlPlaneAuditResponse(
  string EventId,
  DateTimeOffset OccurredAtUtc,
  string ActorSubjectId,
  string ActorIdentityProvider,
  string Action,
  string ResourceType,
  string ResourceId,
  string Descriptor,
  string? PreviousValue,
  string? NewValue,
  string CorrelationId);

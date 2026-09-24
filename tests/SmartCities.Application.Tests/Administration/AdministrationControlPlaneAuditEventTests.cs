using SmartCities.Application.Administration;
using Xunit;

namespace SmartCities.Application.Tests.Administration;

public sealed class AdministrationControlPlaneAuditEventTests
{
  [Fact]
  public void Audit_event_requires_canonical_actor_and_bounded_machine_fields()
  {
    var entry = AdministrationControlPlaneAuditEvent.Create(
      eventId: "audit-001",
      townHallId: "town-hall-a",
      occurredAtUtc: new DateTimeOffset(
        2026, 9, 24, 9, 30, 0, TimeSpan.Zero),
      actorSubjectId: "provider:tenant:official-1",
      actorIdentityProvider: "provider",
      action: AdministrationAuditActions.FeatureFlagSet,
      resourceType: AdministrationAuditResourceTypes.FeatureFlag,
      resourceId: "citizen-mobility",
      descriptor: "citizen-mobility",
      previousValue: "true",
      newValue: "false",
      correlationId: "corr-001");

    Assert.Equal(
      "provider:tenant:official-1",
      entry.ActorSubjectId);
    Assert.Equal(
      AdministrationAuditActions.FeatureFlagSet,
      entry.Action);
    Assert.Equal("false", entry.NewValue);
  }

  [Fact]
  public void Audit_event_rejects_control_characters_in_persisted_descriptors()
  {
    Assert.Throws<ArgumentException>(
      () => AdministrationControlPlaneAuditEvent.Create(
        eventId: "audit-001",
        townHallId: "town-hall-a",
        occurredAtUtc: DateTimeOffset.UtcNow,
        actorSubjectId: "provider:tenant:official-1",
        actorIdentityProvider: "provider",
        action: AdministrationAuditActions.WhitelistRuleEnsure,
        resourceType: AdministrationAuditResourceTypes.WhitelistRule,
        resourceId: "rule-1",
        descriptor: "email:official@example.com\nforged",
        previousValue: null,
        newValue: null,
        correlationId: "corr-001"));
  }
}

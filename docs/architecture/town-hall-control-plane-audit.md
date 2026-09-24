# Town Hall control-plane audit trail

## Purpose

SmartCities records completed changes to the local Town Hall control plane so authorized administrators can answer:

- who changed it;
- when the change completed;
- which canonical identity provider authenticated the actor;
- what control-plane resource changed;
- what bounded machine value changed;
- which request correlation identifier can be used to investigate operational telemetry.

The audit trail is not an authentication source and does not grant authority.

## Audited mutations

The initial audit scope is deliberately limited to completed control-plane writes:

```text
feature-flag.set
administration-whitelist.ensure
administration-whitelist.delete
administration-grant.ensure
administration-grant.delete
```

Idempotent whitelist/grant creation is modeled as `ensure`. If the requested normalized rule/grant already exists, no mutation occurs and no new completed-change event is appended.

## Event contract

Each immutable event contains:

```text
eventId
townHallId
occurredAtUtc
actorSubjectId
actorIdentityProvider
action
resourceType
resourceId
descriptor
previousValue?
newValue?
correlationId
```

Actor subject/provider are derived from the already-canonicalized request principal. Clients cannot submit them in mutation bodies.

Correlation comes from the privacy-safe `X-Correlation-ID` boundary already established by request observability.

## Transactional guarantee

For the SQLite control plane, the business mutation and audit insert use the **same SQLite transaction**.

Therefore:

```text
configuration/whitelist/grant write succeeds
AND audit insert succeeds
→ COMMIT

otherwise
→ ROLLBACK
```

This avoids the failure mode where a control-plane change succeeds but its audit record is silently lost.

Read-only audit history is provided by a dedicated append-only store contract.

There is no application or HTTP update/delete operation for audit rows.

## Access

Audit history:

```text
GET /api/administration/audit?limit=100
```

requires:

- canonical authentication;
- canonical subject;
- canonical authority role;
- current Town Hall Administration admission;
- `administration-audit.read`.

The query is newest-first and bounded to 1–200 events per request.

## Data classification

Audit records may contain Administration access-control metadata such as:

- staff email addresses used in whitelist/grant selectors;
- municipal email domains;
- canonical subjects;
- canonical role/permission identifiers.

Therefore audit history is protected Administration data, not public configuration.

The audit trail must never persist:

- passwords;
- tokens;
- cookies;
- Authorization headers;
- private keys;
- client secrets;
- request bodies;
- citizen evidence payloads;
- private model reasoning.

## UI

Town Hall Administration shows the latest 50 events when the current canonical session has `administration-audit.read`.

After a successful control-plane mutation, the UI refreshes the audit panel without treating browser state as authoritative.

## Current scope

This increment does not yet provide:

- retention/archival policy;
- export;
- cryptographic chaining/signatures;
- external SIEM forwarding;
- audit search/filtering;
- attempted/failed authorization-event logging.

Those can be added without changing the immutable event contract.

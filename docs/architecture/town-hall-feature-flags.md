# Per-Town-Hall control plane and feature flags

## Decision

SmartCities uses two deliberately separate persistence planes.

```text
operational/domain data
  -> SQL Server or PostgreSQL
  -> reports, Evidence Cases, decision reviews, future domain state

deployment control plane
  -> SQLite
  -> non-sensitive per-Town-Hall settings
  -> feature flags
  -> typed Administration access-control metadata
```

SQLite in this design is not a new domain database provider. The existing rule that production domain persistence uses SQL Server or PostgreSQL remains unchanged.

## Town Hall identity

Each deployment provides:

```text
SmartCities:TownHall:Id
```

The identifier is stable machine data and is used as the partition key for local control-plane state.

The dedicated SQLite connection is provided through:

```text
SmartCities:Configuration:ConnectionString
```

Development example:

```text
TownHallId = local-town-hall
Data Source=smartcities.configuration.db
```

## Non-sensitive configuration store

`INonSensitiveConfigurationStore` is intentionally narrow:

```text
GetAsync(townHallId, key)
SetAsync(townHallId, key, value)
```

The SQLite table is keyed by:

```text
(town_hall_id, setting_key)
```

Values are plaintext.

Therefore this store MUST NOT contain:

- passwords;
- client secrets;
- signing keys;
- access or refresh tokens;
- private keys;
- citizen evidence or PII;
- authentication/session material;
- any value whose disclosure would create a security or privacy incident.

Secret configuration continues to come from deployment secret mechanisms and must not be copied into this database.

The same SQLite file may also contain typed Administration whitelist records in a separate table. Exact staff email addresses can be personal data, so those records do **not** pass through `INonSensitiveConfigurationStore` and must not be described as non-sensitive settings. They remain local access-control metadata.

## Feature registry

Feature IDs are code-owned machine contracts. An administrator cannot create arbitrary new feature IDs through the API.

The initial registry contains:

| Feature ID | Default | Purpose |
| --- | --- | --- |
| `citizen-mobility` | enabled | F3 mobility report → reviewed outcome vertical slice |

The default is enabled to preserve backward compatibility with the already-certified F3 deployment. Every future vertical slice must register an explicit default.

Persisted overrides use the generic non-sensitive store, scoped by Town Hall.

## Public discovery

```text
GET /api/system/features
```

Returns only:

- current Town Hall ID;
- known feature IDs;
- effective enabled state.

The response is non-sensitive and `no-store`.

React uses this endpoint as the authoritative deployment capability snapshot. Disabled slices are not rendered.

## Protected management

```text
PUT /api/system/features/{featureId}
{ "enabled": true | false }
```

Mutation requires both:

- admission to Town Hall Administration through the current whitelist/bootstrap rules;
- the canonical permission:

```text
feature-flags.manage
```

under policy:

```text
smartcities.feature-flags.manage
```

Provider-native roles/scopes do not bypass the canonical identity boundary.

## Backend gating

Vertical-slice HTTP surfaces use `RequireFeatureAttribute`.

When the feature is disabled, the request returns HTTP 404 before the controller action executes. This prevents a hidden frontend link from being the only enforcement mechanism.

A configuration-store failure is not interpreted as enabled. The request fails rather than executing a slice whose deployment state cannot be established.

## Scope

This increment provides the platform mechanism and gates the existing `citizen-mobility` slice.

It does not yet provide:

- a graphical administrator console;
- remote centralized configuration;
- secret storage;
- percentage rollouts;
- user-targeted experiments;
- scheduled flag changes;
- cross-Town-Hall shared databases.

Those can be added without changing the domain persistence boundary.

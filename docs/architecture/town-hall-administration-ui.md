# Town Hall Administration UI

## Route

The product web client exposes the Administration shell at:

```text
/administration
```

The shell is a client of existing backend contracts. It does not make authorization decisions independently.

## Bootstrap and admission states

At startup the UI resolves both:

```text
GET /api/authentication/session
GET /api/administration/access
```

and renders an explicit state:

- anonymous + usable bootstrap → bootstrap initialization;
- anonymous + initialized whitelist → configured sign-in providers;
- authenticated + not admitted → access denied;
- authenticated + admitted → Administration workspace.

The access probe is `no-store`. Bootstrap is advertised only when the whitelist is empty **and** the deployment bootstrap credential is configured.

No admission state is persisted to localStorage.

## Bootstrap initialization

The UI uses the fixed bootstrap login identifier:

```text
townhalladmin@smartcities.local
```

and submits the entered password only to:

```text
POST /api/administration/bootstrap/session
```

The password is never stored by the product client.

The bootstrap session can create the first whitelist rule. Immediately after that mutation the UI re-queries backend admission. Because the backend revokes bootstrap admission as soon as the whitelist becomes non-empty, the Administration shell disappears and normal whitelisted sign-in is required.

## Feature permissions: operation vs configuration

Feature operation and feature configuration are separate capabilities.

Operational access to a vertical slice requires both:

```text
feature-flags.manage
<feature-id>.manage
```

For the current mobility slice:

```text
feature-flags.manage
citizen-mobility.manage
```

This is the capability used by staff who operate the slice and attend citizen work. Fine-grained action permissions remain additional; for example finalizing a mobility review also requires `decision-review.finalize`.

Changing deployment feature state is different. Configuration requires both:

```text
feature-flags.config
<feature-id>.config
```

For the current mobility slice:

```text
feature-flags.config
citizen-mobility.config
```

All admitted administrators can view the public feature snapshot. A toggle is enabled only when the current canonical session has an authority role plus both configuration grants for that specific feature.

The backend policy remains authoritative and can still return 403. An operational `manage` grant never authorizes feature configuration.

## Persisted authorization grants

Town Hall Administration can persist canonical roles and permissions independently of static IdP mappings.

Grant targets use the same portable selectors as admission:

- `email-domain`;
- `email`;
- `canonical-subject`.

Grant values are restricted to a code-owned catalog. Arbitrary role or permission strings cannot be persisted through Administration.

Persisted grants are applied **per request after provider/session canonicalization**. They are not written into browser-local authority state. Consequently, adding or removing a grant takes effect on the next request without requiring a new login.

Persisted grants are effective only while the identity is currently admitted by the Administration whitelist. Provider-derived canonical grants remain independent and additive.

Bootstrap receives `administration-grants.manage` while the whitelist is empty, allowing the initial operator to pre-provision grants for the future municipal domain/email/subject before adding the first whitelist rule. The first whitelist record still revokes bootstrap admission immediately.

## Administration whitelist

Whitelist contents are loaded only when the current canonical session has:

- at least one authority role;
- `administration-whitelist.manage`.

The UI supports the same portable rule kinds as the backend:

- `email-domain`;
- `email`;
- `canonical-subject`.

Every add/delete re-queries Administration admission. Deleting the last rule can therefore revoke the current normal administrator and restore bootstrap eligibility, subject to bootstrap deployment configuration.

## Failure behavior

The UI fails closed:

- unavailable session/access state does not render Administration controls;
- 401/403 from protected APIs does not get overridden client-side;
- failed mutations trigger an authoritative admission refresh;
- offline state does not preserve stale Administration authority.

## Scope

This increment is deliberately an Administration v1 shell. It does not introduce:

- provider branding/wrappers;
- user directory browsing;
- external directory synchronization;
- audit-log UI;
- remote centralized configuration;
- secret management.

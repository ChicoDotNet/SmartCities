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

## Feature flags

All admitted administrators can view the public feature snapshot.

Mutation is enabled in the UI only when the current canonical session has:

- at least one authority role;
- `feature-flags.manage`.

The backend policy remains authoritative and can still return 403.

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
- role/permission assignment;
- audit-log UI;
- remote centralized configuration;
- secret management.

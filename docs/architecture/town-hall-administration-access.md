# Town Hall Administration admission and whitelist

## Purpose

Authentication answers **who proved control of an identity**.

The Administration whitelist answers a separate question:

> Is this canonical identity allowed to enter the administrative control plane for this Town Hall?

SmartCities keeps those boundaries separate. Successful Google, Entra, Auth0, Cognito, local, Apple, Facebook, LinkedIn, X, or other authentication does not by itself grant Administration access.

## Portable admission rules

Each Town Hall can configure zero or more rules in its SQLite control-plane database.

Supported v1 rule kinds:

| Machine kind | Example | Match semantics |
| --- | --- | --- |
| `email-domain` | `@townhallname.gob.mx` | Exact canonical email domain, case-insensitive |
| `email` | `official.person@gmail.com` | Exact canonical email, case-insensitive |
| `canonical-subject` | `entra:tenant:subject` | Exact canonical SmartCities subject, case-sensitive |

Leading `@` on a domain is accepted and normalized away.

Wildcards, regexes, URL paths, and suffix guessing are deliberately unsupported. A rule for `townhall.gob.mx` does not silently authorize `sub.townhall.gob.mx`.

This avoids turning an administrative allowlist into a pattern-matching language.

## Canonical email boundary

SmartCities defines:

```text
urn:smartcities:identity:email
```

as an optional canonical identity claim.

Only an authentication-provider adapter can populate it.

The generic OIDC adapter defaults to:

```text
EmailClaimType = email
EmailVerifiedClaimType = email_verified
RequireVerifiedEmail = true
```

An unverified email is omitted from the canonical identity and therefore cannot satisfy email/domain Administration rules.

A deployment may explicitly select another authoritative provider claim and disable the verification-claim requirement when its identity contract warrants that decision.

Provider-native claims never flow directly into the whitelist matcher.

For providers that do not reliably expose a trusted email, use `canonical-subject`.

## Bootstrap administrator

When the whitelist contains **zero rules**, exactly one code-owned bootstrap subject is admissible:

```text
login:   townhalladmin@smartcities.local
subject: local-bootstrap:default:townhalladmin@smartcities.local
```

Bootstrap login is exposed at:

```text
POST /api/administration/bootstrap/session
```

The password comes from deployment secret configuration:

```text
SmartCities:Administration:Bootstrap:Password
```

It is never stored in SQLite and there is no universal production password. When configured it must contain at least 16 characters.

The bootstrap credential endpoint has a dedicated fixed-window rate limit of five attempts per minute. This is intentionally conservative because the endpoint exists only for initial/recovery administration, not normal user traffic.

When configured, the bootstrap credential can establish the same encrypted `SmartCities.Session` cookie used by the rest of the product, even before another interactive provider is enabled.

### Automatic revocation

The bootstrap identity is evaluated against SQLite on every Administration authorization.

As soon as the first whitelist rule exists:

- a new bootstrap login returns 404;
- an already-issued bootstrap cookie no longer satisfies Administration authorization;
- feature-flag management and whitelist management reject the bootstrap session;
- ordinary logout remains available.

If every whitelist rule is deliberately deleted later, bootstrap eligibility returns, but the deployment secret is still required.

## Administration access probe

```text
GET /api/administration/access
```

is public and `no-store`.

It exposes only:

- Town Hall ID;
- whether the current canonical session is admitted;
- whether empty-whitelist bootstrap is currently available.

It does not return the whitelist itself.

## Whitelist management

```text
GET    /api/administration/whitelist
POST   /api/administration/whitelist
DELETE /api/administration/whitelist/{ruleId}
```

requires:

1. canonical authentication;
2. current Administration admission;
3. canonical authority role presence;
4. explicit permission:

```text
administration-whitelist.manage
```

under policy:

```text
smartcities.administration-whitelist.manage
```

Feature Flag mutation similarly requires both current Administration admission and `feature-flags.manage`.

Whitelist membership therefore grants entry to the Administration plane but does not synthesize unrelated SmartCities permissions.

## Persistence and privacy

Whitelist records live in a dedicated SQLite table keyed by Town Hall.

They are deliberately separate from `non_sensitive_settings`.

An exact personal email address is personal/access-control metadata. Operators must protect the SQLite file and its backups accordingly.

SQLite still MUST NOT contain:

- passwords;
- bootstrap password;
- client secrets;
- signing keys;
- private keys;
- access/refresh tokens;
- session material;
- citizen evidence;
- other secret credentials.

## Multi-provider compatibility

Domain/email rules work across providers whenever the adapter can produce a trusted canonical email.

`canonical-subject` works independently of email and is the fallback for provider-specific identities.

Examples:

```text
Google verified email
  -> canonical email
  -> email/domain rule

Entra authoritative preferred_username
  -> explicit adapter configuration
  -> canonical email
  -> email/domain rule

Apple / OAuth provider without durable email
  -> canonical subject
  -> canonical-subject rule
```

Provider wrappers added later must continue to converge on the same canonical identity boundary.

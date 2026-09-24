# Frontend authentication discovery and local login

SmartCities exposes only the login methods that are enabled for the current deployment.

## Provider discovery

```text
GET /api/authentication/providers
```

The response is frontend-safe. It contains provider identity/display metadata plus relative login links and never contains client secrets, signing keys, authorities with credentials, or raw provider configuration.

A local provider is represented as a `credentials` login mode and exposes:

```text
POST /api/authentication/local/session
POST /api/authentication/local/token
```

A redirect provider is represented as a `redirect` login mode and exposes:

```text
GET /api/authentication/providers/{providerId}/challenge
```

React can therefore render exactly the buttons/forms that are valid for the current installation.

## Safe redirect challenge

The challenge endpoint accepts an optional `returnUrl` query value.

Only local application paths beginning with one `/` are accepted. Scheme-relative URLs, absolute external URLs, backslash-based paths, and control characters are rejected. The validated path becomes the post-login redirect carried by ASP.NET Core authentication properties.

## Local credential boundary

SmartCities does not define or persist a password database in the API host.

Deployments provide an implementation of:

```text
ILocalCredentialAuthenticator
```

It receives username/password and returns an already authenticated `ExternalAuthenticatedIdentity` or null. The default implementation always rejects credentials.

This allows local credentials to live in a deployment-selected store while keeping password verification out of Controllers and domain code.

Passwords are never included in request observability because request bodies are excluded from the default operational log.

## Browser session

A successful local session request:

1. verifies the local credentials through `ILocalCredentialAuthenticator`;
2. passes the validated external identity through the existing local adapter and canonicalizer;
3. stores only the resulting canonical SmartCities principal in the encrypted `SmartCities.Session` cookie;
4. returns a minimal canonical session response.

The response is marked `no-store`.

## JWT bearer issuance

A successful local token request performs the same credential verification/canonicalization, then issues a short-lived HMAC-SHA256 JWT using the configured local issuer/audience/signing key.

The token carries the original validated local subject plus the canonical authority-role and permission values using the configured local claim names. When the token is used later, the existing JWT handler validates its signature/lifetime and sends it back through the local adapter/canonicalizer before authorization.

The default JWT lifetime is 60 minutes and may be configured with:

```text
SmartCities:Authentication:Local:JwtLifetimeMinutes
```

Valid range: 1–1440 minutes.

Token responses are marked `no-store`.

## Security notes

- Invalid credentials return HTTP 401 and issue neither cookie nor token.
- When the local provider is disabled, local session/token endpoints return HTTP 404.
- Credential-store implementations should use a modern password hashing function and deployment-specific lockout/rate-limiting policy; those storage policies are intentionally outside this provider-neutral HTTP slice.
- JWT signing keys remain deployment secrets and must never be committed.


## Canonical session lifecycle

The product web client does not keep an independent authentication truth.

On initial load, refresh, direct navigation, reconnection, or return from an interactive OIDC callback it requests:

```text
GET /api/authentication/session
```

The endpoint deliberately returns HTTP 200 for both states. Anonymous state is represented as `authenticated=false` with no subject/provider/authorization material. Authenticated state is reconstructed from the already validated canonical request principal and exposes only the canonical SmartCities subject, provider ID, authority roles, and permissions. Raw provider claims and tokens are not returned. Responses are marked `no-store`.

React models session state explicitly as `checking`, `anonymous`, `authenticated`, or `unavailable`. It does not render the sign-in surface until current-session has resolved anonymous, avoiding a false sign-in flash after refresh or OIDC return. When connectivity is unavailable, the UI does not continue presenting cached browser state as authoritative authentication.

Local credential sign-in still establishes the encrypted shared `SmartCities.Session` cookie, but the UI re-reads current-session before displaying authenticated state. All configured interactive OIDC providers already use that same cookie as their sign-in scheme, so the same bootstrap path recognizes their successful callbacks without provider-specific frontend state.

### Logout and CSRF boundary

Browser logout uses:

```text
POST /api/authentication/session/logout
X-SmartCities-Request: browser
```

The non-simple same-origin request header, together with the session cookie's SameSite policy and the absence of permissive cross-origin credential handling, prevents an ordinary cross-site HTML form from issuing the mutation. Missing markers fail with HTTP 403.

A valid logout request is idempotent. It expires the shared SmartCities browser cookie when the session scheme is configured and still returns HTTP 204 when no interactive provider/session scheme is enabled. React then re-reads current-session before displaying anonymous state.

This operation is intentionally local to SmartCities. It does not trigger global/federated sign-out at Google, Entra, Auth0, Cognito, Apple, or other identity providers. The encrypted ASP.NET Core cookie is a self-contained authentication ticket; this slice expires the browser's canonical cookie but does not claim centralized server-side revocation of a separately copied ticket before its normal expiry.

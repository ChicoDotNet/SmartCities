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

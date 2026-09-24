# Configurable authentication provider reference patterns

SmartCities can register several identity providers at the same time. Provider configuration is evaluated independently.

## Provider enablement semantics

A provider is off when its section is absent or `Enabled=false`.

A provider section that exists with no explicit `Enabled` value is treated as configuration intent and must be complete. Incomplete security configuration fails host composition rather than silently disabling the provider.

All enabled providers converge on the canonical SmartCities identity/authorization model.

## Local cookie + JWT reference

The local reference provider registers:

```text
SmartCities.Session
SmartCities.Local.Jwt
```

`SmartCities.Session` is the shared encrypted browser-session cookie. The future local credential-verification endpoint can issue this cookie after producing a canonical SmartCities identity.

`SmartCities.Local.Jwt` validates locally issued API bearer tokens and canonicalizes their subject/role/permission claims through the local provider adapter.

Example configuration:

```json
{
  "SmartCities": {
    "Authentication": {
      "Local": {
        "Enabled": true,
        "Issuer": "https://identity.example.gov",
        "Audience": "smartcities-api",
        "SigningKey": "<deployment secret, at least 32 bytes>",
        "CookieName": "smartcities.session",
        "SubjectClaimType": "sub",
        "RoleClaimType": "role",
        "PermissionClaimType": "permission",
        "AllowedAuthorityRoles": [
          "citizen",
          "mobility-reviewer"
        ],
        "AllowedPermissions": [
          "decision-review.finalize"
        ]
      }
    }
  }
}
```

The signing key is a deployment secret and must not be committed.

The local adapter promotes role/permission claim values only when they are explicitly present in the configured allowlists. Defaults may also be configured when appropriate for the deployment.

## Generic OpenID Connect reference

Any number of generic OIDC providers can be configured beneath:

```text
SmartCities:Authentication:OpenIdConnect:{providerId}
```

Each provider gets its own challenge/callback scheme:

```text
SmartCities.Oidc.{providerId}
```

All interactive OIDC providers sign their successfully canonicalized identity into the shared `SmartCities.Session` cookie.

Example:

```json
{
  "SmartCities": {
    "Authentication": {
      "OpenIdConnect": {
        "workforce": {
          "Enabled": true,
          "DisplayName": "Workforce SSO",
          "Authority": "https://identity.example.gov",
          "ClientId": "<client id>",
          "ClientSecret": "<deployment secret>",
          "CallbackPath": "/signin-oidc-workforce",
          "SubjectClaimType": "sub",
          "TenantClaimType": "tid",
          "RoleClaimType": "groups",
          "PermissionClaimType": "scope",
          "DefaultAuthorityRoles": [
            "citizen"
          ],
          "RoleMappings": {
            "external-reviewers": "mobility-reviewer"
          },
          "PermissionMappings": {
            "review.finalize": "decision-review.finalize"
          }
        }
      }
    }
  }
}
```

The generic adapter uses explicit allowlist-style mappings: an external role/group/scope value becomes a SmartCities authority role or permission only when the configuration maps it. Unmapped external values are ignored.

This pattern is suitable for standards-compliant OIDC providers such as many Entra ID, Google Identity, Amazon Cognito, Auth0, Apple, and other deployments, subject to each provider's exact protocol/claim capabilities. Provider-specific wrappers can add specialized behavior without changing the canonical authorization model.

Facebook, LinkedIn, X, and any provider whose desired login flow is OAuth-specific rather than compatible with this generic OIDC contract should use a dedicated concrete adapter while still registering into the same provider registry.

## Parallel routing

Request authentication uses a provider-neutral router.

- Bearer requests use `SmartCities.Local.Jwt` when the local provider is enabled.
- Browser/session requests use `SmartCities.Session` when any interactive provider is enabled.
- When no provider can authenticate the request, the existing no-provider scheme produces a safe 401 challenge.

OIDC login challenges are issued explicitly against the provider's `SmartCities.Oidc.{providerId}` scheme, allowing several providers to coexist.

## Secret boundary

The provider registry exposes only provider ID, display name, provider kind, and registered scheme names. Client secrets and signing material never enter the public registry.

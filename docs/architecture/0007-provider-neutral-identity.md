# 0007 — Provider-neutral identity and authorization boundary

## Status

Accepted.

## Decision

SmartCities authorization consumes a small canonical claim vocabulary owned by SmartCities rather than claim names emitted directly by any authentication provider.

The first canonical claims are:

- `urn:smartcities:identity:subject`;
- `urn:smartcities:identity:authority-role`;
- `urn:smartcities:identity:permission`;
- `urn:smartcities:identity:provider`.

The first consequential permission is:

```text
decision-review.finalize
```

and the first authorization policy is:

```text
smartcities.decision-review.finalize
```

That policy requires:

1. an authenticated principal;
2. exactly the canonical authorization surface expected by SmartCities;
3. a canonical subject claim;
4. an authority-role claim;
5. the explicit `decision-review.finalize` permission.

## Provider adapters

Authentication providers remain replaceable.

A future adapter for Microsoft Entra ID, Auth0, Keycloak, municipal SSO, another OpenID Connect provider, or another supported authority owns:

- authentication protocol and token validation;
- issuer/tenant/provider validation;
- mapping of external subject identity into a globally stable SmartCities subject;
- mapping of provider roles/groups/scopes into canonical SmartCities authority-role and permission claims;
- adding the canonical identity-provider claim for audit context.

Provider-native claims such as raw `sub`, `role`, group IDs, or provider-specific scope names are not authorization contracts for application code.

This prevents Controllers and application services from becoming coupled to one identity vendor.

## Stable subject identity

An OpenID Connect `sub` value is generally scoped to its issuer and must not automatically be treated as globally unique across providers.

Provider adapters must therefore produce a stable canonical `SmartCitiesClaimTypes.Subject` value whose uniqueness rules include the provider/tenant context needed by the deployment.

The domain `HumanAuthority.SubjectId` receives this canonical value, never an unqualified provider-native subject guessed by application code.

## Human authority

After a request has satisfied the relevant authorization policy, the API may map the canonical subject and authority-role claims into the existing `HumanAuthority` domain value.

The mapping deliberately performs no authorization itself; policy evaluation and domain accountability remain separate responsibilities.

Ambiguous canonical subject/authority-role claims are rejected rather than selecting an arbitrary value.

## Current scope

No concrete authentication scheme is registered by this decision.

The current citizen mobility endpoints remain unchanged and public for the first vertical slice. No human-review HTTP endpoint exists yet.

`UseAuthorization()` is enabled so policy metadata can be enforced when protected endpoints arrive. The authentication provider and `UseAuthentication()` will be introduced only with the first concrete provider adapter or provider-neutral authentication composition slice.

## Consequences

- multiple providers can feed one stable authorization model;
- permissions are application capabilities, not provider group names;
- identity-provider metadata can be retained without making the provider itself an authorization grant;
- domain code remains free of ASP.NET Core and external IdP SDK types;
- future provider adapters are testable against the same canonical policy contract.

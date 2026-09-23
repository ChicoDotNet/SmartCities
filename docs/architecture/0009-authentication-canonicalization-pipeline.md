# 0009 — ASP.NET Core authentication canonicalization pipeline

## Status

Accepted.

## Context

SmartCities now has provider-neutral authentication-adapter contracts and provider-neutral authorization policies, but the ASP.NET Core request pipeline still needs an explicit trust handoff between a concrete authentication mechanism and canonical SmartCities authorization.

## Decision

The request authentication sequence is:

```text
concrete ASP.NET Core authentication scheme
        ↓
validated external principal
+ IValidatedExternalAuthenticationFeature
        ↓
SmartCitiesAuthenticationCanonicalizationMiddleware
        ↓
IAuthenticationCanonicalizer
        ↓
exactly one IAuthenticationProviderAdapter
        ↓
CanonicalIdentity
        ↓
canonical-only ClaimsPrincipal
        ↓
ASP.NET Core authorization
```

## Authentication registration without a provider

`AddSmartCitiesAuthenticationCanonicalization()` calls ASP.NET Core `AddAuthentication()` but configures no default scheme.

This establishes the host authentication infrastructure without selecting Entra ID, Auth0, Keycloak, cookies, municipal SSO, or another provider.

A future provider integration may add one or more concrete schemes while preserving this canonicalization layer.

## Trusted feature handoff

After a concrete scheme validates a credential/session, its integration must populate `IValidatedExternalAuthenticationFeature` with:

- validated `AuthenticationProviderContext`;
- validated `ExternalAuthenticatedIdentity`.

This feature is a trusted host-integration handoff. It must not be populated from unvalidated request headers or body values.

## Fail-closed behavior

Anonymous requests bypass canonicalization and remain available to public citizen endpoints.

An authenticated request fails closed with HTTP 401 when:

- no trusted external-authentication feature is present;
- the provider context cannot resolve to exactly one adapter;
- the selected adapter returns a canonical identity for a different `ProviderId`.

Unexpected provider-adapter implementation/runtime failures are not converted into authentication failures; they continue as operational errors so infrastructure defects are observable rather than disguised as invalid user credentials.

## Principal replacement

After successful canonicalization, the middleware replaces `HttpContext.User` with a new canonical SmartCities principal.

Provider-native claims are therefore unavailable to subsequent authorization/application code. Authorization sees only SmartCities canonical subject, identity-provider, authority-role, and permission claims.

## Ordering

The host pipeline order is:

```text
UseAuthentication()
UseSmartCitiesAuthenticationCanonicalization()
UseAuthorization()
```

The order is security-significant.

## Current scope

No concrete authentication scheme or provider adapter is registered yet.

The existing public citizen vertical slice continues to run anonymously. This increment only creates the composition boundary that future provider integrations plug into.

# 0008 — Multi-provider authentication adapter contract

## Status

Accepted.

## Context

SmartCities authorization now depends only on canonical claims, but concrete identity providers emit different issuers, tenant identifiers, claim names, role/group models, and permission semantics.

The authentication boundary must therefore support several providers simultaneously without allowing those provider-specific concepts to leak into Controllers, application services, or domain code.

## Decision

Authentication is split into two distinct trust stages.

### Stage 1 — credential validation

A concrete host authentication scheme validates the protocol and credential. Examples may later include OpenID Connect/JWT, cookies, municipal SSO, or another supported mechanism.

This stage owns cryptographic/token/session validation and produces:

- the authentication scheme;
- a validated issuer;
- an optional validated tenant/realm/directory;
- a validated provider-native subject;
- provider-native claims.

### Stage 2 — SmartCities canonicalization

An `IAuthenticationProviderAdapter` explicitly decides whether it accepts that validated provider context and asynchronously converts the external identity into a `CanonicalIdentity`.

The canonical identity contains only:

- stable SmartCities `IdentityProvider`;
- globally stable SmartCities `SubjectId`;
- canonical authority roles;
- canonical SmartCities permissions.

Provider-native claims are input to the adapter, not application authorization claims.

## Adapter resolution

`AuthenticationProviderAdapterResolver` receives all configured adapters and requires exactly one match.

It fails closed when:

- no adapter accepts the validated issuer/tenant context;
- more than one adapter accepts it;
- configured adapters reuse the same SmartCities `ProviderId`.

This prevents fallback/first-match behavior from silently assigning trust to the wrong identity provider.

## Canonical subject

An adapter is responsible for constructing a globally stable SmartCities subject.

For example, an adapter may combine its stable provider identity, validated tenant/realm, and provider-native subject. The exact format belongs to the adapter, but application code never guesses it from raw `sub` values.

## Canonical principal

`CanonicalIdentity.ToClaimsPrincipal(...)` creates a new authenticated principal containing only SmartCities canonical claims.

Raw provider roles, groups, scopes, and claims are intentionally not copied across this boundary.

## Async normalization

Normalization is asynchronous and cancellation-aware because some providers may require trusted entitlement/group resolution after credential validation.

The contract does not require such network access; it merely avoids forcing a breaking interface change if a concrete adapter needs it later.

## Current scope

No concrete authentication provider, JWT bearer handler, cookie scheme, login UI, token authority, or provider SDK is introduced by this ADR.

The next provider-specific increment can implement one or more adapters against this contract and compose them into the host without changing authorization policies or domain contracts.

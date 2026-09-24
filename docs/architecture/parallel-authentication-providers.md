# Parallel authentication provider configuration

SmartCities authentication is designed for multiple providers to operate at the same time. Provider choice is a deployment capability, not a compile-time decision.

## Target provider families

The planned provider adapters are:

- local forms-style sign-in backed by a browser cookie session, with JWT bearer support for APIs;
- Microsoft Entra ID;
- Microsoft personal accounts;
- Google Identity;
- Amazon Cognito;
- Auth0;
- Sign in with Apple;
- Facebook Login;
- LinkedIn;
- X.

All successful providers converge on the same `AuthenticationProviderContext → IAuthenticationProviderAdapter → CanonicalIdentity` boundary. Authorization therefore remains unchanged as providers are added or removed.

## Configuration rule

Each concrete provider is independently registered.

A provider is **off** when:

- its configuration section is absent; or
- it is explicitly disabled.

A provider is **on** when its complete required configuration is available.

A provider that is explicitly enabled, or whose section indicates configuration intent, but is missing required security-critical values must fail host startup rather than silently disable itself.

Examples of security-critical values include issuer/authority, client/application identifier, required secret/certificate/key material where applicable, callback configuration, and trusted tenant/realm constraints.

## Parallel operation

All enabled schemes are registered concurrently. SmartCities does not require one global identity vendor.

Provider routing/canonicalization continues to fail closed:

- the credential is first validated by its concrete ASP.NET Core authentication scheme;
- the trusted external-authentication feature identifies the validated scheme/issuer/tenant;
- exactly one SmartCities provider adapter must accept that context;
- the provider-native principal is replaced by the canonical SmartCities principal before authorization.

## Secrets

Client secrets, signing keys, certificates, and private key material are deployment secrets and must not be committed to the repository.

Local development may use explicit development-only values where a provider supports that safely, but production provider secrets belong in the hosting platform's secret/configuration system.

## Forms and JWT terminology

For ASP.NET Core, the local web experience should use cookie authentication rather than the legacy ASP.NET `FormsAuthentication` implementation. A local credential flow may issue the cookie for browser sessions and JWTs for API/non-browser clients while still canonicalizing both through the same SmartCities identity boundary.

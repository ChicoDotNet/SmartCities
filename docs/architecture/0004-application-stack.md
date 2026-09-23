# 0004 — Reference application stack

## Status

Accepted for project foundation.

## Backend

- .NET 10
- ASP.NET Core APIs
- standard `.resx` localization
- OpenAPI for public API contracts
- deterministic domain behavior isolated from I/O
- future Criterio E-Kernel Core integration through a NuGet adapter

## Frontend

- React
- TypeScript
- Vite
- Fluent UI as the primary component/control system
- Bootstrap for grid, responsive layout, and utilities

Bootstrap must not introduce a second competing widget language where Fluent UI provides the needed control.

## Localization flow

```text
Resources.resx / Resources.es-MX.resx
                |
                v
       ASP.NET Core localization
                |
                v
         JSON resource API
                |
                v
        React localization store
                |
                v
    Fluent UI + Bootstrap surface
```

## Initial solution direction

The first executable architecture is expected to include independently testable projects/modules equivalent to:

- `SmartCities.Api`
- `SmartCities.Domain`
- `SmartCities.Contracts`
- `SmartCities.Criterion`
- `SmartCities.HumanOversight`
- `SmartCities.Infrastructure`
- `SmartCities.Web`

Exact project boundaries may be refined by the first TDD slice when executable evidence justifies a change.

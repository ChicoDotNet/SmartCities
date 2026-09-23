# 0004 — Reference application stack

## Status

Accepted for project foundation.

## Bootstrap scope

The repository begins with one **C# / .NET 10 class library**, `SmartCities.Core`.

There is no ASP.NET Core host, product web application, or database in the bootstrap increment. Those surfaces are added only when an executable contract requires them.

## Planned backend

- .NET 10
- ASP.NET Core APIs
- standard `.resx` localization
- OpenAPI for public API contracts
- deterministic domain behavior isolated from I/O
- future Criterio E-Kernel Core integration through a NuGet adapter

## Planned product frontend

- React
- TypeScript
- Vite
- `.tsx` for React components
- `.ts` for non-visual TypeScript
- no application `.jsx` / plain `.js` source when TypeScript can express the same code
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


## Project website

The GitHub Pages project website is a separate public communication surface. It uses React + TypeScript + Vite with Fluent UI controls and Bootstrap layout/utilities. It is not the future municipal application.

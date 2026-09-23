# 0003 — Localization resource API

## Status

Accepted for project foundation.

## Decision

Canonical market-facing application resources are stored in ASP.NET Core `.resx` resource sets and exposed to the React client as JSON.

The browser does not maintain a second independent translation source of truth.

## Resource layout

Initial direction:

```text
src/SmartCities.Api/
  Resources/
    SharedResources.resx
    SharedResources.es-MX.resx
    CitizenResources.resx
    CitizenResources.es-MX.resx
    MobilityResources.resx
    MobilityResources.es-MX.resx
```

Neutral resources are canonical English. `es-MX` is the first market override.

## API shape

A localization endpoint should expose resolved resources, for example:

```http
GET /api/localization/{locale}/{resourceSet}
```

Illustrative response:

```json
{
  "locale": "es-MX",
  "resourceSet": "CitizenResources",
  "fallbackLocale": "en",
  "resources": {
    "ReportMobilityIssue": "Reportar un problema de movilidad",
    "TrackMyCase": "Dar seguimiento a mi reporte"
  }
}
```

The final route and DTO names are subject to TDD contract design.

## Resolution rules

- Validate supported locale and resource-set identifiers.
- Resolve culture through standard .NET localization mechanisms.
- Fall back deterministically to neutral English when a key is absent.
- Return stable keys; translated values never become persisted identifiers.
- Cache safely where appropriate.
- Do not expose server-only or confidential resources through public endpoints.

## React consumption

React loads JSON into a localization provider/store. Components request values by stable keys.

Fluent UI remains the primary control system. Bootstrap is used for layout/utilities, not as a competing component design system.

## Why server-owned resources

This keeps localization aligned with APIs, supports shared validation/error language, centralizes market content, and avoids drifting duplicate translation catalogs between .NET and React.

## Test obligations

The implementation slice must cover locale resolution, `es-MX` values, neutral fallback, invalid resource-set rejection, missing-key behavior, and stable key semantics.

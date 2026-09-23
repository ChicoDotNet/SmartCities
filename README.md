# SmartCities

SmartCities is an open-source, citizen-first suite for municipalities, urban planners, transport teams, public-space teams, civic technology builders, researchers, and integrators.

The product follows one architectural principle:

> Citizen experiences should stay simple while urban evidence, models, recommendations, uncertainty, and human authority remain traceable behind them.

The project is licensed under **GNU Affero General Public License v3.0**.

## Product architecture

SmartCities is organized into four capability planes:

1. **Citizen Experience** — Digital Town Hall, requests and cases, participation, wayfinding, notifications, accessibility, and transparent decision explanations.
2. **Urban Intelligence** — geospatial context, evidence, surveys, photogrammetry, demand, traffic, simulation, appraisal, audit, and open indicators.
3. **Mobility & Public Realm** — integrated mobility, public transport, rail, interurban and multimodal mobility, cycling, walking, accessibility, road safety, traffic impact, stations, wayfinding, public life, curb, parking, and urban freight.
4. **Extensible Smart City Domains** — housing, environment, water, waste, energy, resilience, civil protection, digital inclusion, local economy, and tourism.

See [the canonical module catalog](docs/product/module-catalog.md).

## Application stack

The initial reference application uses:

- .NET 10 / ASP.NET Core APIs;
- `.resx` / `Resources.resx` localization on the server;
- JSON localization endpoints consumed by the web client;
- React + TypeScript + Vite;
- Fluent UI as the primary control system;
- Bootstrap for layout and utilities.

See [application stack](docs/architecture/0004-application-stack.md) and [localization resource API](docs/architecture/0003-localization-resx-api.md).

## Decision-support boundary

SmartCities does **not** embed proprietary reasoning knowledge.

The public application depends on an explicit criterion port. During bootstrap, it will use a deterministic mock. When **Criterio E-Kernel Core** publishes its first NuGet release, an adapter can implement the same port without changing citizen-facing or urban-domain modules.

See [Criterion Kernel port](docs/architecture/0002-criterion-kernel-port.md).

## Human authority: IAIA(oh)

The project uses the operating concept **IAIA(oh)**: *Inteligencia Aumentada por IA, observada por humanos* — AI-augmented intelligence observed and governed by humans.

AI may organize evidence, compare alternatives, identify uncertainty, generate traceable recommendations, and help explain decisions. It does not become the competent legal, technical, administrative, or political authority merely because it produced a recommendation.

Consequential workflows preserve evidence provenance, assumptions, uncertainty, alternatives, material objections, accountable human authority, and final human disposition.

## Language and localization

Code, identifiers, schemas, tests, ADRs, and canonical documentation are written in **English** so the project can support cities globally.

User-facing content is localized by market. The first market is **Mexico**, so the first shipped citizen and operator content will be **Spanish (Mexico), `es-MX`**.

See [localization and market content](docs/product/localization.md).

## Public repository boundary

This repository may contain generic domain models, lawful-to-publish algorithms, public schemas, interoperability adapters, reusable UI, deterministic mocks, synthetic data, properly licensed public data, tests, benchmarks, and public documentation.

It must not contain proprietary expert corpora, protected training material, confidential municipal/customer data, citizen PII, identifiable mobility traces, secrets, credentials, or material that cannot be redistributed under the project license.

See [system context and boundaries](docs/architecture/0001-system-context-and-boundaries.md).

## First executable slice

The first vertical slice is intentionally narrow:

`Digital Town Hall → Report mobility issue → Evidence Case → Mock Criterion Kernel → Human Review → Explainable Citizen Outcome`

See [first vertical slice](docs/product/first-vertical-slice.md).

## Delivery

SmartCities follows the CaliWood-style ASBN delivery topology:

`working branch → squash → dev → squash → main → content-neutral main→dev sync`

See [delivery governance](docs/governance/delivery.md).

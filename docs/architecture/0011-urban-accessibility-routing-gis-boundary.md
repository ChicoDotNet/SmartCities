# 0011 — Urban accessibility routing and GIS boundary

## Status

Accepted for V1.1.

## Context

Access to the City / Urban Accessibility needs routing and geospatial capabilities, but SmartCities must not become coupled to one routing engine, GIS SDK, transit provider, or proprietary spatial model.

The vertical must ultimately support two outcomes:

```text
citizen: origin + destination/category -> understandable journey
official: origin zone + destination category + threshold -> accessibility coverage
```

The architecture must allow deterministic development, a real reference engine, and third-party replacement while preserving provenance and accessibility limitations.

## Decision

### 1. SmartCities owns normalized product semantics

SmartCities will own the public concepts needed to express:

- origin/destination references;
- destination categories;
- journey;
- leg;
- normalized mode;
- transfer;
- duration;
- walking distance;
- accessibility known/unknown state;
- zone/threshold accessibility analysis;
- source/freshness/provenance;
- assumptions, limitations, and uncertainty.

Executable DTOs are introduced incrementally through TDD rather than frozen by this ADR.

### 2. Routing remains behind a public port

A later V1 increment will introduce the routing port.

Conceptual shape:

```text
Urban Accessibility domain/application
        |
        v
IAccessibilityRoutingEngine
        |
        +-- deterministic implementation
        +-- real reference adapter
        +-- customer/community adapters
```

The port returns SmartCities contracts.

Provider-specific request/response types, errors, capabilities, authentication, and version handling remain inside `SmartCities.Integrations` or the concrete adapter package.

### 3. Capability differences are explicit

Routing engines may differ in supported modes, wheelchair-aware routing, matrix queries, elevation, transfer semantics, or other capabilities.

SmartCities must use capability discovery or explicit adapter configuration when those differences become observable.

It must not silently pretend an unsupported capability exists.

### 4. GIS is an input/output boundary, not the domain owner

The domain may use small geospatial primitives and references required by the product.

V1.2 will define the minimum point/bbox/path/zone/CRS/distance semantics.

Map rendering, tile systems, GIS SDK objects, spatial database provider types, or visualization-specific geometry must not leak into the public product contracts.

### 5. GTFS is transit input, not a SmartCities-invented schema

V1 uses GTFS Schedule where scheduled transit data is required.

GTFS ingestion, validation, diagnostics, provenance, and freshness are implemented in their dedicated increment.

GTFS Realtime remains primarily V2 scope.

### 6. Accessibility uncertainty is first-class

Routing/accessibility output must be able to say that accessibility is unknown.

Unknown source data is not interpreted as accessible.

Later profiles may add walking thresholds and other supported needs, but V1 does not make claims beyond available evidence.

### 7. Privacy is minimized

Origin/destination pairs can expose movement intent.

This ADR does not authorize persistence of journey queries.

Later API/observability work must explicitly decide which request details are necessary to retain and should prefer bounded/non-identifying diagnostics.

### 8. Feature lifecycle

`urban-accessibility` is registered disabled by default during construction.

The feature may be promoted to enabled-by-default only through a deliberate product decision after an executable, documented, real-integration-backed MVP exists.

## Reference adapter direction

OpenTripPlanner is the current candidate for the first real routing adapter because it provides a separable routing-engine boundary suitable for the planned walking + public-transport use case.

This ADR does not claim an OpenTripPlanner integration exists.

Before implementation, the adapter increment must verify current upstream interfaces, deployment/version compatibility, licensing, timeout/cancellation behavior, health semantics, and contract replay.

## Consequences

Positive:

- V1 product semantics remain stable across routing engines;
- deterministic TDD is possible without external infrastructure;
- GIS/rendering choices can evolve independently;
- unknown accessibility data remains truthful;
- providers can be substituted without changing citizen/official contracts.

Costs:

- adapter mapping and capability documentation are mandatory;
- some engine-specific features may remain intentionally unavailable until represented by a justified public contract;
- isochrone/coverage normalization may require semantic compromise that must be documented.

## Non-goals

This ADR does not implement:

- geospatial primitives;
- City Context;
- GTFS ingestion;
- routing engine port;
- deterministic routing;
- OpenTripPlanner adapter;
- journey API/UI;
- official analysis;
- maps.

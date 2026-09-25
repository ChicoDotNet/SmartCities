# Urban Accessibility — V1 product contract

## Purpose

The `urban-accessibility` vertical answers two related questions over the same governed mobility evidence.

Citizen:

> How can I reasonably reach a place I need in the city?

Official:

> Which people or areas cannot reasonably reach important destinations?

V1.1 defines the product boundary and governance contract only. Routing, GIS primitives, City Context data, GTFS ingestion, adapters, APIs, and UI arrive in later increments.

## Feature identity and default

Stable feature ID:

```text
urban-accessibility
```

The feature is registered **disabled by default** while the vertical has no executable citizen or official path.

Operational permission pair:

```text
feature-flags.manage
urban-accessibility.manage
```

Configuration permission pair:

```text
feature-flags.config
urban-accessibility.config
```

Those grants do not create additional authority by themselves. Later official workflows may require narrower permissions when a consequential action is introduced.

## Citizen MVP contract

A citizen can provide either:

```text
origin + explicit destination
```

or:

```text
origin + destination category
```

and receive at least one practical alternative containing walking and public transport when the configured data/engine can produce one.

The result must expose, where available:

- total duration;
- walking distance;
- transit legs;
- transfers;
- normalized journey legs/modes;
- accessibility information explicitly marked known/unknown;
- source/provenance;
- source/model/feed freshness;
- known assumptions and limitations.

The MVP does not promise universal routing coverage or perfect accessibility knowledge.

## Official MVP contract

An authorized official can provide:

```text
origin zone
+ destination category
+ duration threshold
```

and receive an accessibility analysis that distinguishes:

- areas meeting the threshold;
- areas outside the threshold;
- population affected when a suitable population dataset exists;
- territory affected when the geometry supports it;
- network/data/model sources;
- assumptions;
- freshness;
- limitations and uncertainty.

The analysis is decision support. It is not an automatic governmental disposition.

## Ubiquitous vocabulary

The following terms are canonical product concepts. Their exact C# representation is intentionally deferred until the increment that needs executable behavior.

### Origin

The starting place for a journey query or the starting zone/reference for an official analysis.

### Destination

An explicit target place.

### Destination category

A City Context category representing a class of useful destinations, for example healthcare, education, public services, employment, or another municipality-defined category.

A category is not a routing-engine type.

### Journey

A normalized SmartCities representation of a feasible movement alternative.

A journey can contain multiple legs and modes without exposing provider-specific routing DTOs.

### Leg

One contiguous movement segment using one normalized mode.

### Transfer

A transition between public-transport legs or services. Walking access/egress is represented explicitly rather than silently counted as a transfer.

### Accessibility knowledge

A result must distinguish known accessibility information from unknown/missing information.

Unknown data must never be converted into a positive accessibility claim.

### Accessibility threshold

A bounded, explicit travel-time threshold used by official analysis. Units and time semantics must be unambiguous.

### Zone

A municipal/analysis-area reference. Polygon/CRS semantics are introduced in the geospatial-primitives and City Context increments rather than invented here.

### Provenance

The information needed to understand which source/feed/network/model/adapter and effective time contributed to a result.

## Product boundaries

SmartCities owns:

- citizen and official query/result contracts;
- normalized journeys/legs/modes;
- geospatial references/primitives needed by the vertical;
- destination categories and City Context references;
- source/freshness/provenance semantics;
- accessibility known/unknown semantics;
- assumptions and limitations;
- official accessibility analysis;
- evidence linkage;
- feature governance;
- citizen/official explanation.

Specialized systems may own:

- graph search;
- transit routing algorithms;
- timetable planning;
- isochrone computation;
- network preprocessing;
- other specialized routing/GIS algorithms.

## Routing integration boundary

A later increment will introduce a public port conceptually equivalent to:

```text
IAccessibilityRoutingEngine
  -> deterministic implementation
  -> reusable contract suite
  -> real reference adapter
  -> custom adapters
```

The name and executable DTOs remain subject to TDD when V1.5 starts.

Provider-specific types, including OpenTripPlanner or another routing engine's models, must remain inside its adapter.

OpenTripPlanner is a candidate first real reference adapter, not a domain dependency and not an implemented capability in V1.1.

## GIS boundary

The map is a visualization, not the domain.

V1 will introduce only geospatial semantics proven necessary for the citizen and official outcomes:

- point;
- bounding box;
- line/path;
- zone/polygon reference;
- CRS semantics;
- coordinate validation;
- distance semantics.

A GIS SDK, tile provider, renderer, or spatial database must not define SmartCities public domain types.

## Transit data boundary

GTFS Schedule is the preferred open transit-schedule interchange format for V1 where applicable.

SmartCities should ingest/normalize the information needed by the vertical rather than invent a proprietary equivalent of GTFS.

Realtime transit belongs to V2 unless a later V1 decision demonstrates a minimal dependency.

## Evidence and provenance

Journey/accessibility results must preserve enough information to explain:

- relevant source/feed/network;
- source/version identifier when available;
- adapter/model version when applicable;
- effective/observed/imported time as applicable;
- freshness;
- transformations;
- assumptions;
- limitations;
- quality/uncertainty where supplied.

Evidence Hub links results and inputs without requiring provider-specific payloads to become domain contracts.

## Privacy assumptions

Origins, destinations, and accessibility needs can reveal sensitive movement intent.

V1.1 introduces no journey persistence.

Later executable increments must apply data minimization and must not:

- place real citizen origins/destinations in repository fixtures;
- persist identifiable movement traces merely for convenience;
- log raw request coordinates in normal diagnostic telemetry without an explicit privacy design;
- infer accessibility support when the source data is unknown;
- require biometric identity or raw sensing material.

Demo Town examples remain synthetic.

## Localization and accessibility

Canonical technical contracts and documentation are English.

Citizen/operator copy will ship through the existing localization pipeline with neutral English and `es-MX`.

The product must distinguish accessible/known, inaccessible/known, and unknown data states in a way that is understandable to assistive-technology users and does not rely only on color or map geometry.

## Explicit V1 non-goals

V1 does not require:

- realtime transit;
- cycling;
- shared mobility/GBFS;
- universal metropolitan-scale matrix optimization;
- advanced disability-specific personalization;
- scenario/investment comparison;
- generic sensor ingestion;
- video analytics;
- parking analytics;
- a universal digital twin;
- replacement of specialized routing/GIS engines.

Those capabilities belong to later verticals or later increments when an observable product need justifies them.

## V1.1 Definition of Done

V1.1 is complete when:

- `urban-accessibility` is a code-owned registered feature;
- default state is disabled;
- canonical manage/config permissions are derivable and assignable;
- citizen and official MVP outcomes are explicit;
- vocabulary and non-goals are explicit;
- routing/GIS/provider boundaries are accepted;
- privacy/no-overclaim rules are explicit;
- architecture and feature-management docs match executable registration;
- exact-head CI is green.

## V1.2 — Geospatial primitives

V1.2 implements the small SmartCities-owned geospatial vocabulary required by the vertical:

- `CoordinateReferenceSystem`;
- `GeoPoint`;
- `GeoBoundingBox`;
- `GeoPath`;
- `ZonePolygonReference`;
- `Distance` + explicit `DistanceSemantics`.

Canonical semantics and limitations are documented in [Urban Accessibility geospatial primitives](../architecture/urban-accessibility-geospatial-primitives.md).

The increment deliberately adds no GIS library, map, routing behavior, polygon engine, coordinate transformation, spatial persistence, or distance calculation.

## V1.3 — City Context v1

V1.3 adds the smallest immutable city-context vocabulary required for citizen destinations and later official accessibility analysis:

- destination categories;
- zones backed by external polygon references;
- points of interest/destinations;
- pedestrian and public-transport network references;
- transit stop references;
- source/version/freshness provenance;
- referentially consistent `CityContextSnapshot`.

Canonical semantics and limitations are documented in [Urban Accessibility City Context v1](../architecture/urban-accessibility-city-context.md).

V1.3 deliberately adds no persistence, API, GIS analysis, GTFS ingestion, network topology, routing, or Demo Town data. Unknown source version/effective time remains explicitly unknown rather than fabricated.

## V1.4 — Demo Town v1

V1.4 provides the first executable synthetic municipality fixture under [`samples/demo-town/`](../../samples/demo-town/README.md).

It contains:

- one synthetic municipal boundary;
- three synthetic zones;
- healthcare, education, and public-services destination categories;
- four synthetic POIs;
- a small GeoJSON pedestrian line network;
- three public-transport stops;
- one synthetic bus route with two scheduled trips in GTFS Schedule;
- deterministic source/version/effective/retrieval metadata.

The repository tests load the distributed sample artifacts and reconstruct a valid V1.3 `CityContextSnapshot`, while separately checking basic GTFS referential consistency.

GeoJSON and GTFS remain external fixture formats. Their DTO/file models do not become SmartCities domain contracts.

V1.4 deliberately implements no routing, production GTFS ingestion, API, UI, persistence, map rendering, network topology, or real municipal data.

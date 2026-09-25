# SmartCities nine-vertical release program

## Purpose

This document turns SmartCities domain expansion into a concrete release program.

The program deliberately starts with the most ambitious citizen/municipal capabilities and progressively reuses the contracts, datasets, integration seams, operational conventions, and documentation created by earlier verticals.

The goal is not to create nine unrelated menu items.

The goal is to reach a state where a municipality can:

- deploy SmartCities with only the verticals it needs;
- authenticate officials with one or more identity providers;
- admit officials through portable Town Hall whitelist rules;
- assign canonical roles and permissions locally;
- configure verticals independently;
- connect municipal or third-party mobility systems through documented public ports;
- expose useful citizen experiences;
- preserve provenance, human authority, privacy, and control-plane auditability;
- replace reference integrations without modifying domain logic.

The corresponding open-source goal is equally important:

> A competent engineer who has never spoken with the SmartCities maintainers should be able to clone the repository, run a useful demonstration, understand one vertical, connect a supported external engine or data source, and implement a new adapter by following the repository documentation and contract tests.

## Stable baseline before the nine verticals

Before V1 starts, the currently certified Digital Town Hall and Administration platform should be promoted from `dev` to `main` as a stable baseline.

That baseline contains, at minimum:

- F3 Digital Town Hall citizen mobility case flow;
- canonical identity and provider-neutral authentication;
- Town Hall whitelist/admission;
- per-Town-Hall feature flags;
- `manage` versus `config` authorization semantics;
- persisted Town Hall role/permission grants;
- Administration UI;
- transactional control-plane audit trail;
- PostgreSQL/SQL Server domain persistence support;
- SQLite non-sensitive local control-plane persistence.

Promotion remains:

```text
working branch
  -> squash merge to dev
  -> exact-head certification
  -> dev -> main promotion PR
  -> squash merge to main
  -> exact-state verification
  -> content-neutral main -> dev sync
```

Each of the nine verticals receives one deliberate `main` promotion when it reaches its Definition of MVP.

Intermediate increments merge only to `dev`.

---

# Program-wide Definition of MVP

A vertical does not reach MVP because an API, screen, or adapter exists.

A vertical reaches MVP only when all applicable dimensions below are satisfied.

## 1. Citizen outcome

At least one meaningful citizen task is complete end to end.

The task must produce information or an outcome that is understandable without requiring transport-engineering expertise.

## 2. Official outcome

At least one meaningful municipal task is complete end to end.

The official must be able to understand, inspect, configure, prioritize, review, or act on the same underlying domain information used by the citizen experience.

## 3. Evidence and provenance

Material outputs identify, as applicable:

- data source;
- effective date/time;
- acquisition method;
- dataset/model version;
- data-quality state;
- assumptions;
- known limitations;
- provenance;
- uncertainty.

## 4. Interoperability

Specialized external systems remain behind public SmartCities ports.

Domain and UI code must not depend directly on provider-specific types.

## 5. Reference implementation

Where an external engine or standard is material to the vertical:

- a deterministic fake/reference implementation exists for TDD;
- at least one real adapter exists;
- the adapter passes a reusable compatibility/contract suite;
- known semantic gaps are documented.

## 6. Governance

The vertical has:

- a registered feature flag;
- appropriate `<feature>.manage` permissions;
- appropriate `<feature>.config` permissions;
- human authority where consequential decisions exist;
- Administration configuration where applicable;
- control-plane audit coverage for configuration changes.

## 7. Localization and accessibility

Citizen/operator-facing content ships in:

- canonical neutral English resources;
- `es-MX`.

Material UI paths meet the repository's accessibility requirements.

## 8. Real E2E

The MVP path crosses real runtime boundaries appropriate to the vertical.

Mocks alone are insufficient when the core value proposition depends on an external engine, feed, database, or protocol.

## 9. Operational readiness

The vertical documents and validates:

- configuration;
- health/readiness;
- observability;
- expected failure modes;
- retries/timeouts where appropriate;
- degraded/fallback behavior;
- deployment assumptions;
- privacy considerations.

## 10. Third-party reproducibility

An engineer outside the original delivery team can reproduce the MVP and replace or implement an integration using only public repository material.

This dimension is mandatory.

---

# Documentation is part of the product

Documentation is a release artifact, not post-delivery cleanup.

Every vertical should converge on a structure similar to:

```text
docs/
  product/
    <vertical>.md

  architecture/
    <vertical>.md

  integrations/
    <integration>/
      README.md
      quickstart.md
      configuration.md
      contract.md
      troubleshooting.md
      operations.md
      compatibility.md

  development/
    <vertical>-local-development.md
    <vertical>-testing.md

  operations/
    <vertical>-runbook.md

  tutorials/
    <vertical>-from-zero.md
    build-your-own-<adapter>.md

samples/
  demo-town/
    ...
```

The exact layout may evolve when repetition justifies common templates, but the information content is required.

## Adapter documentation contract

Every external adapter must answer:

1. What problem does the adapter solve?
2. Which SmartCities port does it implement?
3. Which external product/protocol/specification versions are supported?
4. What prerequisites are required?
5. How is the external system started or reached locally?
6. What is the minimum configuration?
7. How does health/readiness work?
8. What does SmartCities send?
9. What does the external system return?
10. Which external semantics are intentionally not mapped?
11. How are timeouts handled?
12. How is cancellation handled?
13. What are the failure/error categories?
14. How are retries handled, if at all?
15. Which data may be sensitive?
16. What licensing/deployment obligations may apply?
17. How are compatibility tests executed?
18. What fixtures are provided?
19. How can the adapter be replaced by a custom one?
20. What are the most common troubleshooting cases?
21. How is the adapter upgraded to a new upstream version?

## Three documentation audiences

### A. Evaluator / adopter

Goal:

```text
clone
-> prerequisites / doctor
-> run Demo Town
-> open browser
-> perform citizen workflow
-> perform official workflow
```

The repository should make first useful evaluation achievable without tribal knowledge.

### B. Newly hired implementation engineer

Goal:

```text
architecture overview
-> vertical overview
-> local setup
-> data flow
-> configuration
-> tests
-> runbook
-> deployment
-> troubleshooting
```

A newly assigned engineer should be able to become productive without requiring continuous pairing with the original authors.

### C. External integrator / contributor

Goal:

```text
public port
-> semantic contract
-> fixture set
-> contract test suite
-> reference adapter
-> build-your-own-adapter tutorial
-> compatibility requirements
-> contribution checklist
```

The project should make independent third-party integrations plausible.

---

# Integration Contract Replay

External integrations should use a common compatibility method.

```text
Port
  -> deterministic fake/reference implementation
  -> reusable contract test suite
  -> reference adapter
  -> replay fixtures
  -> capability discovery
  -> real integration E2E
```

Compatibility work follows the repository rule:

```text
PRESERVE
-> REPLAY
-> DIVERGE
-> EXPLAIN
-> FIX
-> VERIFY
-> REPEAT
-> PROMOTE
```

A C# interface by itself is not sufficient to claim interoperability.

An interoperable integration requires:

- documented semantics;
- fixtures;
- contract tests;
- configuration examples;
- diagnostics;
- known limitations;
- replacement instructions.

## Expected public ports

The program is expected to introduce ports such as:

```text
IAccessibilityRoutingEngine
ITransitRealtimeSource
ISharedMobilitySource
IDemandModel
ITrafficModel
IMicrosimulationEngine
IAppraisalEngine
IMobilityDataSource
IPhotogrammetrySource
```

These names are planning targets, not frozen public APIs. Each becomes stable only through normal design/TDD governance.

Provider-specific types must not escape their adapters.

---

# Cross-cutting provider-agnostic mobility integration program

This is a transversal capability of V1–V9, **not a tenth vertical**.

Canonical architecture:

```text
physical world
  -> observations / datasets
  -> evidence
  -> models
  -> scenarios
  -> recommendation / decision support
  -> human authority
  -> operation
  -> measured outcome
```

SmartCities owns public contracts, normalized semantics, provenance, evidence linkage, workflow, comparison, authority, auditability, and citizen/official explanation.

External systems may own sensing, routing, assignment, simulation, CAD/BIM, rail/station engineering, photogrammetry, or other specialized algorithms.

The architectural contract is defined by [ADR 0010](../architecture/0010-provider-agnostic-mobility-integrations.md) and the contributor-facing rules in [docs/integrations](../integrations/README.md).

## Raw sensing material versus derived urban observation

The program distinguishes:

```text
raw sensing material
  !=
derived urban observation
```

When an outcome can be supported by a derived observation, SmartCities should not retain source video/images or other high-risk raw material.

Examples of useful provider-neutral derived semantics include:

- mobility counts and classifications;
- direction;
- speed;
- occupancy/density;
- parking occupancy;
- bounded safety events;
- observation interval/time;
- location/geometry;
- acquisition method;
- unit;
- quality/confidence;
- provenance;
- privacy classification.

Facial recognition, face matching, biometric identity, identifiable mobility traces, and unrelated surveillance capabilities are outside the normal mobility-domain boundary.

## Aggregated mobility data

The program should support, when required by a vertical:

- origin/destination observations and matrices;
- volume profiles;
- speed profiles;
- route/flow information;
- temporal and spatial aggregation;
- mode/class;
- uncertainty/quality;
- provenance;
- privacy classification.

Prefer aggregated information when person-level traces are unnecessary.

## External models, simulations, and engineering artifacts

SmartCities must support two legitimate execution patterns:

```text
SmartCities scenario
  -> provider-neutral port
  -> external execution
  -> normalized results/artifacts
  -> evidence/scenario comparison
```

and:

```text
externally executed model
  -> result/artifact import
  -> normalized SmartCities references
  -> evidence/scenario comparison
```

Provider/model/version, assumptions, uncertainty, provenance, generated artifacts, and result semantics must remain explicit.

Specialized engineering artifacts are referenced and consumed; proprietary CAD/BIM/rail/station formats are not copied into SmartCities domain contracts.

## Integration mechanisms

Adapters may use the mechanism that best fits the source:

- REST/JSON;
- CSV/file import;
- webhook/event ingestion;
- OGC SensorThings API for heterogeneous IoT observations when appropriate;
- OGC API – Features for geospatial features when appropriate;
- GTFS Schedule and GTFS Realtime for transit-specific data.

No standard is forced where its semantic model does not fit.

## Integration rollout by vertical

The provider-agnostic capability is paid for by concrete product needs:

| Vertical | Integration responsibility |
| --- | --- |
| V1 | Routing port, GTFS Schedule, pedestrian/network inputs, real routing adapter. |
| V2 | GTFS Realtime and operational transit sources. |
| V3 | External model/simulation lifecycle, deterministic provider, execution/import paths, normalized results/artifacts. |
| V4 | Introduce only the minimum source-neutral safety/event observations required by Safety Cases. |
| V5 | Generalize urban observations and mobility datasets from proven needs; add synthetic sensing, counts, video-derived observations, parking occupancy, OD/flow datasets, public indicators, and standards-based ingestion where it fits. |
| V6 | Reuse V5 observations plus routing/safety for cycling and shared mobility. |
| V7 | Reuse observations/GIS for public-space and public-life measurements. |
| V8 | Reuse journey/place/accessibility information; no new provider infrastructure by default. |
| V9 | Reference/version external engineering, field-survey, photogrammetry, and asset artifacts where required by operations. |

This preserves the program rule against speculative horizontal infrastructure.

## Commercial compatibility backlog

Commercial-product compatibility is a backlog of **candidate adapters**, not a support claim.

Examples include count/sensing platforms, video-analytics platforms, transportation-model suites, pedestrian/station simulators, and rail/engineering tools. Candidate families may include products such as Eco-Counter, intuVision, Bentley/CUBE, LEGION, and OpenRail.

Before any such adapter is marked Implemented:

1. current official documentation or a lawful customer integration contract must be available;
2. the actual API/file/event/SDK mechanism must be confirmed;
3. licensing and redistribution restrictions must be reviewed;
4. credentials and non-redistributable proprietary material must remain outside the public repository;
5. the adapter must pass the reusable SmartCities contract suite;
6. semantic gaps and supported versions must be documented.

When access is insufficient, work is limited to generic contracts, deterministic simulations, synthetic fixtures, mapping guidance, or a clearly labeled skeleton.

Compatibility status must use explicit labels:

- Implemented;
- Simulated;
- Documented;
- Proposed;
- Blocked by external access.

---

# Demo Town

A synthetic distributable municipality should grow with the nine verticals.

Canonical location:

```text
samples/demo-town/
```

Expected content over time:

```text
boundary
zones
street references
walk network
points of interest
GTFS
stations
routes
bike network
hazard events
public spaces
sample OD
sample traffic counts
sample projects/scenarios
sample field assets
sample inspections
```

Requirements:

- no citizen PII;
- no protected customer material;
- reproducible source/generation where practical;
- redistribution-compatible licenses for any external source;
- deterministic fixtures where useful;
- documentation connecting each dataset to the verticals that consume it.

Demo Town should ultimately support a developer journey such as:

```text
1. Start Demo Town.
2. Run the target vertical.
3. Replace the sample feed with municipal data.
4. Replace the deterministic engine with a real adapter.
5. Run contract tests.
6. Verify health/readiness.
7. Exercise citizen and official workflows.
```

---

# Vertical release sequence

The program deliberately works from the most ambitious vertical toward the simplest.

This order is intended to force high-value reusable capabilities to emerge from real product needs rather than speculative horizontal infrastructure.

---

# V1 — Access to the City / Urban Accessibility

**Feature ID:** `urban-accessibility`

## Product question

Citizen:

> Can I reach what I need in the city, and what are my practical options?

Official:

> Which people or areas cannot reasonably reach important destinations?

## Citizen MVP

A citizen can select:

- origin;
- explicit destination or destination category;

and receive at least one walking + public-transport alternative with:

- total duration;
- walking distance;
- transit legs;
- transfers;
- known accessibility attributes;
- known data limitations;
- relevant source/update information.

## Official MVP

An official can select:

- origin zone;
- destination category;
- threshold such as 30 minutes;

and obtain an accessibility result showing:

- areas meeting the threshold;
- areas outside the threshold;
- population/territory coverage when data exists;
- routing/data provenance;
- assumptions/limitations.

## Expected reusable capabilities

- geospatial primitives;
- City Context;
- POIs/destination categories;
- zones;
- transport/journey domain;
- GTFS Schedule ingestion;
- routing-engine abstraction;
- deterministic routing;
- first real routing adapter;
- accessibility result model;
- GIS visualization.

## Planned increments

### V1.1 — Product contract and ADR

Define:

- journey;
- origin/destination;
- destination category;
- accessibility query;
- accessibility result;
- provenance;
- limitation/unknown states;
- feature flag;
- manage/config permissions.

The canonical product contract lives in [Urban Accessibility](../product/urban-accessibility.md) and the routing/GIS isolation decision in [ADR 0011](../architecture/0011-urban-accessibility-routing-gis-boundary.md).

The feature is registered disabled by default until an executable citizen/official path exists.

Documentation skeleton is created here, not at the end.

### V1.2 — Geospatial primitives

Introduce only the primitives required by V1:

- point;
- bounding box;
- line/path;
- zone/polygon reference;
- CRS conventions;
- coordinate validation;
- distance units/semantics.

Canonical executable semantics are documented in [Urban Accessibility geospatial primitives](../architecture/urban-accessibility-geospatial-primitives.md).

This increment adds no GIS SDK, map, routing engine, polygon engine, spatial persistence, coordinate transform, or distance calculator.

### V1.3 — City Context v1

Introduce:

- destination/POI categories;
- municipal zones;
- network references;
- source/provenance metadata.

### V1.4 — Demo Town geography

Create the first usable Demo Town:

- municipal boundary;
- zones;
- sample destinations;
- network references;
- documentation.

### V1.5 — `IAccessibilityRoutingEngine`

Define:

- capabilities;
- query/result contract;
- failure taxonomy;
- cancellation;
- time semantics;
- accessibility-known/unknown states.

### V1.6 — Deterministic routing engine

Implement a deterministic reference engine for:

- unit/component tests;
- citizen UX development;
- official-analysis development;
- offline contract replay.

### V1.7 — GTFS Schedule ingestion

Support:

- feed acquisition/import;
- validation;
- agency/routes/stops/trips/calendar;
- provenance;
- diagnostics;
- freshness metadata.

### V1.8 — Pedestrian/network ingestion boundary

Define enough network semantics for:

- walking access to stops;
- walking-only journey;
- routing integration.

### V1.9 — First real routing adapter

Initial reference target may be OpenTripPlanner or another suitable open engine.

Requirements:

- real engine deployment;
- adapter;
- contract replay;
- compatibility document;
- health;
- troubleshooting.

### V1.10 — Citizen journey API

Expose an engine-neutral citizen contract.

### V1.11 — Citizen journey UI

Implement:

- origin/destination;
- destination category;
- itinerary;
- legs/transfers;
- accessibility-known/unknown;
- data freshness/limitations.

### V1.12 — Accessibility profile

Add:

- walking thresholds;
- mobility/accessibility needs supported by available data;
- explicit unknown state;
- no invented accessibility claims.

### V1.13 — Official accessibility analysis

Zone + destination category + threshold → coverage result.

### V1.14 — GIS visualization

Map:

- origins/zones;
- accessible/inaccessible areas;
- relevant destinations;
- result provenance.

### V1.15 — Integration DX

Complete:

- routing-engine integration quickstart;
- configuration;
- adapter tutorial;
- compatibility suite usage;
- troubleshooting;
- Demo Town walkthrough.

### V1.16 — Hardening and MVP promotion

Validate:

- real routing engine E2E;
- GTFS failures;
- degraded behavior;
- performance baseline;
- privacy;
- localization;
- accessibility;
- exact-head CI.

Then promote V1 to `main`.

## Explicit V1 non-goals

Do not block MVP on:

- realtime transit;
- cycling;
- GBFS;
- advanced disability-specific personalization;
- metropolitan-scale matrix optimization;
- scenario comparison.

Those belong to later increments/verticals.

---

# V2 — Public Transport & Multimodal Journey

**Feature ID:** `public-transport`

## Citizen MVP

A citizen can inspect a transit journey with:

- route;
- stops;
- transfers;
- schedule;
- expected arrival when realtime exists;
- service alerts;
- degraded fallback to schedule data;
- accessibility-known/unknown.

## Official MVP

An official can inspect route/stop operations including:

- planned service;
- current/recent realtime condition when available;
- frequency;
- disruptions;
- stop/route coverage;
- transfers;
- accessibility metadata;
- data freshness.

## Planned increments

1. Transit operational domain contract.
2. GTFS quality diagnostics.
3. `ITransitRealtimeSource`.
4. Deterministic realtime source.
5. GTFS-Realtime reference adapter.
6. Arrival/vehicle/service-alert normalization.
7. Citizen realtime journey/stop experience.
8. Official route/stop workspace.
9. Transfer/station accessibility.
10. Stale-feed and fallback semantics.
11. Realtime adapter cookbook/runbook.
12. Exact-head E2E and MVP promotion.

---

# V3 — Scenario & Investment Lab

**Feature ID:** `mobility-scenarios`

## Citizen MVP

A citizen can compare two public alternatives using understandable measures such as:

- estimated cost;
- estimated beneficiaries;
- travel-time effect;
- safety effect;
- material externalities;
- key assumptions;
- uncertainty/limitations.

SmartCities does not automatically declare a political/public-investment winner.

## Official MVP

An official can:

- define a baseline;
- define at least two alternatives;
- attach/version evidence;
- execute at least one demand measure;
- execute at least one operational measure;
- execute a simplified appraisal;
- inspect assumptions and provenance;
- compare results.

## Expected ports

- `IDemandModel`;
- `ITrafficModel`;
- `IMicrosimulationEngine`;
- `IAppraisalEngine`.

## Planned increments

1. Scenario/version domain.
2. Baseline/intervention model.
3. Assumption/constraint/provenance contracts.
4. Provider-neutral model descriptor plus external execution/import boundary.
5. Deterministic external simulation provider for lifecycle TDD.
6. Demand-model port + deterministic model.
7. Demand dataset/import boundary.
8. Traffic/microsimulation ports.
9. First real open reference simulation adapter where technically justified.
10. Simulation job lifecycle, cancellation, idempotency, and failure semantics.
11. Normalized results/artifacts plus Evidence Hub linkage.
12. Appraisal port and technical/economic/social/environmental indicators.
13. Official Scenario Workspace.
14. Citizen scenario comparison.
15. Build-your-own model/simulation provider documentation and replay fixtures.
16. Reproducibility E2E, external-result import proof, and MVP promotion.

Commercial model suites remain optional adapters and cannot define the scenario domain.

---

# V4 — Road Safety & Traffic Impact

**Feature ID:** `road-safety`

## Citizen MVP

A citizen can:

- report a dangerous location;
- see the resulting case;
- see whether it was investigated;
- see a resulting intervention/outcome when public.

## Official MVP

Officials can combine:

- citizen reports;
- crash/safety evidence;
- location/geometry;
- operational observations;

into a Safety Case with:

- hotspot/context;
- priority;
- technical review;
- intervention;
- before/after evidence.

A Traffic Impact Case can represent:

- proposed development/project;
- expected demand;
- affected locations;
- multimodal impacts;
- mitigation commitments;
- review/disposition.

## Planned increments

1. Safety Case domain.
2. Crash/hazard evidence contract.
3. Smallest source-neutral safety/event observation contract required by the Safety Case.
4. Spatial aggregation/hotspots.
5. Safety/risk indicators.
6. Citizen hazard-report extension.
7. Official hotspot queue/map.
8. Intervention lifecycle.
9. Before/after measurement.
10. Traffic Impact Case.
11. Mitigation commitments and verification.
12. External safety/data adapter guide and replay fixtures.
13. E2E and MVP promotion.

Do not generalize a universal sensor platform here. V5 extracts/generalizes only after V4 and earlier verticals provide concrete reuse evidence.

---

# V5 — Mobility Data Observatory

**Feature ID:** `mobility-observatory`

## Why it comes fifth

The Observatory is intentionally not built first.

V1–V4 should reveal the actual recurring data requirements. V5 then extracts/generalizes the platform that those products genuinely require.

## Citizen MVP

Publish understandable privacy-safe indicators around:

- access;
- coverage;
- demand;
- safety;
- travel behavior;
- trends.

## Official MVP

Officials can ingest/catalog/validate/reuse datasets such as:

- counts;
- GTFS;
- OD;
- stated-preference surveys;
- sensors;
- field observations;
- crash/safety datasets.

## Planned increments

1. Observation/dataset catalog with provenance, licensing, privacy classification, and quality dimensions.
2. Canonical mobility-observation contract generalized from actual V1–V4 needs.
3. Deterministic synthetic sensor adapter + reusable ingestion/normalization contract tests.
4. PostgreSQL/SQL Server persistence and bounded query/read model where justified by the observable workflow.
5. REST/JSON, CSV/file, and webhook ingestion patterns with duplicate/idempotency and timestamp/unit normalization semantics.
6. OGC SensorThings compatibility adapter when the observation semantics fit.
7. Count-provider compatibility slice using synthetic provider payloads; evaluate a real commercial adapter separately only after official documentation/licensing review.
8. Video-derived traffic observation slice: classified counts, speed, direction, occupancy/density, and bounded safety events converging on the same canonical contracts without storing raw video.
9. Parking occupancy mapping into the existing Parking, Curb & Urban Freight capability plus evidence/indicator query.
10. OD/flow/volume/speed profile contracts with explicit aggregation, uncertainty, geography, mode, and privacy semantics.
11. Synthetic aggregated mobility-dataset ingestion and scenario/evidence consumption.
12. OGC API – Features boundary for geospatial collections where appropriate.
13. Survey/stated-preference and field-observation compatibility.
14. Official data-quality/provenance workspace.
15. Citizen Open Indicators/public-display/API path consuming approved SmartCities contracts rather than source providers.
16. Build-your-own mobility sensor/data adapter tutorials, replay fixtures, real E2E evidence, and MVP promotion.

The Observatory remains the deliberate point where repeated needs are generalized. It is not permission to build a speculative data lake.

---

# V6 — Cycling & Micromobility

**Feature ID:** `cycling-micromobility`

## Citizen MVP

A citizen can inspect a cycling journey including:

- route;
- known infrastructure type;
- restrictions/known conditions;
- conflict points;
- parking where available;
- shared bikes/scooters when a compatible source exists.

## Official MVP

Officials can inspect:

- cycling network;
- gaps;
- discontinuities;
- counts/demand when available;
- conflicts;
- proposed/intervened segments.

## Planned increments

1. Cycling-network semantics.
2. Cycling routing profile.
3. Infrastructure classifications.
4. Network-gap analysis.
5. Citizen bike journey.
6. Official cycling workspace.
7. `ISharedMobilitySource`.
8. GBFS reference adapter.
9. Safety/field integration.
10. E2E and MVP promotion.

---

# V7 — Public Space & Public Life

**Feature ID:** `public-space`

## Citizen MVP

A citizen can inspect a park/plaza/public space and understand:

- location/access;
- accessibility-known/unknown;
- amenities;
- observed conditions;
- public incidents/issues;
- activities when available.

## Official MVP

Officials can maintain and assess:

- inventory;
- distribution;
- accessibility;
- amenities/quality;
- observed use;
- public-life observations;
- issues;
- interventions.

## Planned increments

1. Public-space domain.
2. Inventory + geometry.
3. Accessibility/amenity attributes.
4. Observation protocol.
5. Public-life counts/observations.
6. Citizen public-space page.
7. Official assessment workspace.
8. Intervention/prioritization.
9. Survey/import cookbook.
10. E2E and MVP promotion.

---

# V8 — Urban Wayfinding

**Feature ID:** `urban-wayfinding`

## Citizen MVP

Journeys can expose meaningful orientation through:

- entrance;
- station;
- platform;
- exit;
- crossing;
- building;
- landmark;
- accessible-path information where known.

## Official MVP

Officials can audit a wayfinding journey:

```text
journey
-> decision points
-> required information
-> existing information/sign
-> missing/incorrect information
-> intervention
```

## Planned increments

1. Wayfinding graph/decision-point domain.
2. Sign/information asset.
3. Journey instruction model.
4. Accessibility semantics.
5. Citizen guidance.
6. Official route audit.
7. Signage/information gap detection.
8. Import/integration documentation.
9. E2E and MVP promotion.

---

# V9 — Field Survey & Asset Operations

**Feature ID:** `field-operations`

## Citizen MVP

Expose only operational information that is materially useful, such as:

- equipment out of service;
- completed installation;
- inaccessible elevator;
- repaired sign;
- activated infrastructure.

## Official MVP

Officials can manage a field asset lifecycle:

```text
asset
-> location
-> field survey
-> evidence
-> inspection
-> installation
-> tests
-> commissioning
-> acceptance
-> incident
```

## Planned increments

1. Field Asset contract.
2. Inspection/checklist domain.
3. Evidence attachment/provenance.
4. External engineering/model artifact reference with source-system, version, geometry/scenario/asset linkage, and privacy/licensing metadata.
5. Commissioning lifecycle.
6. Field-friendly official UX.
7. Photogrammetry/reference linkage.
8. Citizen-visible operational status.
9. Equipment/engineering-provider adapter cookbook with synthetic examples and explicit compatibility states.
10. E2E and MVP promotion.

## Specialized training

Specialized training is not a tenth vertical.

It becomes a cross-cutting capability through:

- playbooks;
- checklists;
- contextual help;
- implementation guides;
- operator tutorials;
- simulation/training scenarios where justified.

---

# Reuse graph

The intended dependency/reuse shape is:

```text
V1 Access to the City
  -> GIS
  -> journey
  -> routing ports
  -> GTFS
  -> accessibility

V2 Public Transport
  -> realtime transit
  -> route/stop operations

V3 Scenario Lab
  -> models
  -> alternatives
  -> simulation
  -> appraisal

V4 Road Safety
  -> spatial evidence
  -> interventions
  -> before/after

V5 Observatory
  -> extracts/generalizes recurring data needs proven by V1-V4

V6 Cycling
  -> reuses routing + safety + observatory

V7 Public Space
  -> reuses GIS + surveys + accessibility

V8 Wayfinding
  -> reuses journeys + places + accessibility

V9 Field Operations
  -> reuses Evidence + GIS + audit + assets
```

This ordering is designed so later verticals become cheaper because earlier verticals have already paid for reusable public contracts.

---

# Documentation Definition of Done per increment

Every PR must answer the following questions.

## Public contract

Did a public contract change?

If yes:

- update canonical contract documentation;
- update examples/fixtures;
- update compatibility expectations.

## Integration

Was an integration added or changed?

If yes:

- quickstart;
- configuration;
- contract semantics;
- compatibility/version matrix;
- health/diagnostics;
- troubleshooting;
- contract tests;
- replacement tutorial.

## Configuration

Was configuration added?

If yes:

- configuration reference;
- defaults;
- validation/fail-fast behavior;
- environment/deployment examples.

## Operations

Was an operational dependency introduced?

If yes:

- readiness;
- failure behavior;
- runbook;
- upgrade/recovery notes.

## UI / workflow

Did a citizen or official workflow change?

If yes:

- product workflow documentation;
- localization;
- accessibility notes where material.

## Architecture

Was a cross-cutting design decision introduced?

If yes:

- ADR or architecture document.

## Limitation

Was a limitation discovered?

If yes:

- document it explicitly;
- do not hide it behind an optimistic abstraction.

Documentation must describe the executable product, not an aspirational future version.

---

# Integration rules

## Do not replace specialized engineering software without evidence

SmartCities should own:

- cases;
- evidence;
- provenance;
- scenarios;
- assumptions;
- normalized outputs;
- comparison;
- workflow;
- authority;
- audit;
- citizen explanation.

Specialized engines may continue to own algorithms such as:

- routing;
- assignment;
- demand estimation;
- microsimulation;
- appraisal;
- photogrammetric processing.

## Provider isolation

Provider-specific types remain inside adapters.

For example:

```text
SmartCities domain
    |
    +-- IAccessibilityRoutingEngine
          |
          +-- DeterministicRoutingEngine
          +-- OpenTripPlannerAdapter
          +-- CustomerSpecificAdapter
```

The same principle applies to simulation, demand, realtime transit, shared mobility, and other external systems.

## Capability discovery

Where providers differ materially, adapters should expose capability metadata rather than forcing the domain to guess.

Examples:

- wheelchair-routing available;
- realtime available;
- elevation available;
- turn restrictions available;
- matrix query available;
- multimodal modes supported;
- max batch size;
- model type/version.

---

# Rules against speculative infrastructure

The program follows five hard constraints.

1. Do not build a horizontal capability solely because it may be useful later. A vertical must require it.
2. Do not reimplement a specialized engine when a port + adapter is the appropriate boundary.
3. Do not promote a vertical whose core real-world outcome exists only against mocks.
4. Do not claim interoperability from an interface alone; require docs, fixtures, contract tests, and demonstrated substitution.
5. Do not allow documentation and executable behavior to diverge.

---

# Estimated program size

The estimates below are intentionally ranges of coherent engineering increments, not calendar commitments.

| Stage | Estimated increments |
| --- | ---: |
| Stable F3/platform baseline promotion | 1–2 |
| V1 Access to the City | 14–16 |
| V2 Public Transport & Multimodal | 10–12 |
| V3 Scenario & Investment Lab | 14–16 |
| V4 Road Safety & Traffic Impact | 11–13 |
| V5 Mobility Data Observatory | 14–16 |
| V6 Cycling & Micromobility | 8–10 |
| V7 Public Space & Public Life | 8–10 |
| V8 Urban Wayfinding | 7–9 |
| V9 Field Survey & Asset Operations | 8–10 |
| **Approximate total** | **95–114** |

Documentation/integration-DX work is included inside the vertical ranges rather than treated as an optional separate phase.

After V1 and V2, observed delivery velocity should replace these planning estimates.

---

# Planned stable promotion history

The target stable history is:

```text
main
 |
 +-- Baseline: F3 Digital Town Hall + Administration platform
 |
 +-- V1 MVP: Access to the City
 |
 +-- V2 MVP: Public Transport & Multimodal Journey
 |
 +-- V3 MVP: Scenario & Investment Lab
 |
 +-- V4 MVP: Road Safety & Traffic Impact
 |
 +-- V5 MVP: Mobility Data Observatory
 |
 +-- V6 MVP: Cycling & Micromobility
 |
 +-- V7 MVP: Public Space & Public Life
 |
 +-- V8 MVP: Urban Wayfinding
 |
 +-- V9 MVP: Field Survey & Asset Operations
```

Every point in this sequence must be:

- deployable;
- documented;
- exact-head certified;
- independently understandable;
- usable without private ASBN repositories.

---

# Criterio E-Kernel

Criterio remains a cross-cutting capability, not a tenth vertical.

The public boundary remains:

```text
ICriterionKernel
    -> MockCriterionKernel
    -> future Criterio E-Kernel Core adapter
```

When the real package is available, integration follows contract replay and must not change public SmartCities domain contracts unnecessarily.

Criterio must not block V1.

It becomes particularly valuable as V3 Scenario & Investment Lab matures, where complex evidence, alternatives, uncertainty, and human review are central.

---

# Immediate next steps

The currently intended execution sequence is:

## Step 0

Merge and certify the provider-agnostic integration architecture/roadmap documentation so the stable baseline records the intended external-system boundary before V1 starts.

## Step 1

Certify the resulting exact `dev` platform baseline.

## Step 2

Promote the current F3 + Administration platform baseline from `dev` to `main`.

## Step 3

Perform the required content-neutral `main -> dev` synchronization.

## Step 4

Start V1 implementation from the synchronized `dev` state:

```text
V1 — Access to the City
Iteration V1.1
Product Contract + ADR + documentation skeleton
+ urban-accessibility feature registration
```

Routing/GTFS contracts remain V1-specific; generic sensing/dataset infrastructure stays deferred until a vertical proves it is required.

## Step 5

Before implementing any commercial adapter, re-check current official documentation, access mechanisms, version support, and licensing. Record the compatibility state without overclaiming.

---

# Success condition for the nine-vertical program

The program succeeds when a municipality can:

1. deploy SmartCities;
2. enable only the verticals it wants;
3. connect its identity providers;
4. admit officials by domain/email/subject;
5. assign local canonical authority;
6. connect municipal datasets and specialized engines through public adapters;
7. provide useful citizen experiences;
8. give officials evidence-backed operational tools;
9. preserve provenance, privacy, human authority, and auditability.

The open-source program succeeds when an unrelated engineer can truthfully say:

> I found SmartCities, followed the repository documentation, ran the demo municipality, connected my own municipal data or engine, executed the contract tests, and implemented a working integration without needing private ASBN knowledge.

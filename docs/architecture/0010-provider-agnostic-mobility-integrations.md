# 0010 — Provider-agnostic mobility integration boundary

## Status

Accepted as an architectural boundary before implementation.

## Context

SmartCities must consume useful mobility observations, datasets, model outputs, and engineering artifacts without making the public domain model depend on a manufacturer, product, commercial SDK, or provider-specific payload.

The target information flow is:

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

Specialized systems may continue to own sensing, routing, assignment, simulation, CAD/BIM, photogrammetry, or other domain-specific algorithms. SmartCities owns the public contracts, normalization boundary, provenance, evidence linkage, workflow, comparison, authority, and citizen/official explanation.

## Decision

### 1. Provider types never enter the domain

Provider-specific DTOs, SDK types, authentication models, error objects, and proprietary identifiers remain inside `SmartCities.Integrations` or an equivalent infrastructure adapter.

The domain receives only SmartCities-owned contracts.

A physical sensor, video-analytics system, municipal API, file import, or future source may all produce compatible canonical observations when their semantics actually overlap.

### 2. Raw sensing material is distinct from derived urban observation

SmartCities explicitly distinguishes:

```text
raw sensing material
  !=
derived urban observation
```

Examples of raw material include video, images, radar returns, or source-specific telemetry.

Examples of derived observations include:

- 12 bicycles northbound during a five-minute interval;
- mean traffic speed for a bounded road segment and interval;
- parking occupancy for a defined facility/curb segment;
- a wrong-way safety event with time, location, quality, and provenance.

When a product outcome can be delivered from derived observations, SmartCities should not retain the raw material.

Biometric identity, face matching, identifiable mobility traces, and unrelated surveillance capabilities are outside the normal SmartCities mobility domain.

### 3. Canonical integration families

Runtime contracts are introduced only when a vertical needs observable behavior. The expected families are:

#### Urban observations

Potential semantics include:

- counts and classifications;
- direction;
- speed;
- occupancy/density;
- parking occupancy;
- bounded safety/event observations;
- observation interval/time;
- geometry/location reference;
- acquisition method;
- unit;
- quality/confidence;
- provenance;
- privacy classification.

No universal observation mega-model is created in advance. V4/V5 will introduce the smallest useful contracts and generalize only after real reuse appears.

#### Aggregated mobility datasets

Potential semantics include:

- origin/destination observations or matrices;
- volume profiles;
- speed profiles;
- route/flow information;
- temporal aggregation;
- geography;
- mode/class;
- quality/uncertainty;
- provenance;
- privacy classification.

Prefer aggregated, non-identifiable information when the use case does not require person-level traces.

#### External models and simulations

SmartCities must be able to:

1. define or reference a scenario;
2. describe required inputs;
3. identify model/provider/version without leaking provider types;
4. execute through an adapter when supported;
5. import results when execution occurs externally;
6. preserve assumptions, provenance, uncertainty, and model version;
7. reference generated artifacts;
8. compare normalized scenario results;
9. attach material outputs to Evidence Hub workflows.

A deterministic provider is required before provider-specific compatibility work so the lifecycle can be tested without proprietary software.

#### External engineering artifacts

SmartCities may reference versioned geospatial, rail, station, CAD/BIM, photogrammetry, digital-twin, or similar engineering artifacts when a vertical requires them.

The domain stores references, provenance, applicable scenario/asset context, geometry references, versions, and usable outputs rather than copying proprietary formats into core contracts.

### 4. Provenance is mandatory for decision-relevant external information

A normalized observation, dataset, model result, or external artifact that can influence a decision must preserve enough metadata to reconstruct, where applicable:

- producing source/system;
- source-local identifier;
- device/system reference;
- observation/effective time;
- received/imported time;
- location/geometry reference;
- acquisition method;
- unit and classification;
- quality/confidence;
- transformations;
- adapter version;
- dataset/model version;
- legally referenceable source artifact;
- privacy classification;
- assumptions and uncertainty.

The existing `EvidenceReference` / `EvidenceProvenance` contracts remain the current minimal evidence boundary. They should be extended only when a concrete vertical proves that additional semantics are necessary.

### 5. Standards are selected by fit, not by fashion

Supported integration mechanisms may include:

- REST/JSON;
- CSV/file import;
- webhook/event ingestion;
- OGC SensorThings API for heterogeneous sensor observations when its model fits;
- OGC API – Features for geospatial feature access when its model fits;
- GTFS Schedule and GTFS Realtime for transit-specific data.

No standard is mandatory merely because it exists. Each adapter must document its semantic mapping and gaps.

### 6. Commercial compatibility requires evidence

A commercial-provider adapter is not considered implemented until:

1. current official documentation or a lawful customer-provided integration contract is available;
2. API/file/event/SDK access is confirmed;
3. redistribution and licensing constraints are understood;
4. secrets and non-redistributable SDK material remain outside the public repository;
5. provider-specific behavior passes the reusable SmartCities contract suite;
6. compatibility limitations are documented.

When those conditions are not met, the repository may contain generic contracts, deterministic fakes, synthetic payloads, mapping guidance, or a clearly labeled skeleton, but it must not claim a working integration.

### 7. Integration maturity is explicit

Documentation uses the following states when provider or protocol compatibility is discussed:

- **Implemented** — executable integration exists and is validated;
- **Simulated** — deterministic/synthetic implementation demonstrates the contract;
- **Documented** — mapping or integration instructions exist but no executable adapter is claimed;
- **Proposed** — candidate integration only;
- **Blocked by external access** — implementation requires documentation, credentials, licensing, software, or data not available to the public project.

### 8. Integration packages are optional

A Town Hall that has no external sensors, analytics, datasets, or simulation tools must run normally.

Provider packages must not become mandatory dependencies of SmartCities core modules.

Feature-specific configuration follows the existing per-Town-Hall control-plane and `<feature>.manage` / `<feature>.config` authorization rules.

### 9. Persistence remains separated

When normalized domain information requires persistence, supported production providers remain PostgreSQL and SQL Server.

The SQLite Town Hall control plane remains reserved for local feature/configuration and Administration control-plane state; it is not the store for mobility observations, datasets, simulation runs, or external engineering artifacts.

## Standards baseline at acceptance

The architecture recognizes these public standards as current candidates:

- OGC SensorThings API Part 1: Sensing 1.1 for heterogeneous IoT observations;
- OGC API – Features Part 1 Core 1.0.1 / related parts for geospatial feature access;
- GTFS Schedule for scheduled public-transport data;
- GTFS Realtime 2.0 semantics for realtime public-transport feeds.

The repository should re-check official upstream documentation before implementing or updating an adapter.

## Consequences

Positive:

- municipal and provider integrations can evolve without contaminating domain contracts;
- sensor technologies can converge on reusable observations;
- simulation vendors can be substituted behind stable ports;
- privacy minimization is an architectural default;
- provenance survives normalization;
- third parties can implement adapters without forking product logic.

Costs:

- adapters must perform explicit semantic mapping;
- not every provider concept will map losslessly;
- capability discovery and compatibility documentation are mandatory where semantics differ;
- some commercial integrations will remain intentionally incomplete until lawful access exists.

## Non-goals

This ADR does not:

- implement a sensor ingestion platform;
- add queues/background processing;
- choose one universal observation schema;
- introduce provider SDK dependencies;
- store raw video;
- introduce facial recognition or biometric identity;
- implement a universal digital twin;
- replace routing, simulation, CAD/BIM, rail, or engineering products;
- claim compatibility with any commercial provider.

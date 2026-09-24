# SmartCities integration architecture

This directory documents how external systems participate in SmartCities without owning SmartCities domain semantics.

## Core rule

```text
external system
  -> provider/protocol adapter
  -> SmartCities-owned contract
  -> evidence / domain workflow
```

Provider-specific DTOs and SDK types stop at the adapter boundary.

## Information flow

The intended system flow is:

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

SmartCities does not need to perform every upstream activity itself. A specialized engine may calculate a route, derive counts from video, execute a transport simulation, or generate an engineering model. SmartCities can consume the normalized result with provenance.

## Integration families

### Urban observations

Future verticals may normalize compatible semantics such as:

- count/classification;
- direction;
- speed;
- occupancy/density;
- parking occupancy;
- bounded safety events;
- time/interval;
- location/geometry;
- acquisition method;
- unit;
- quality/confidence;
- provenance;
- privacy classification.

The canonical model will be introduced incrementally from concrete vertical needs. This document does not define an anticipatory mega-schema.

### Aggregated mobility datasets

Examples include:

- OD matrices/aggregates;
- volume profiles;
- speed profiles;
- route/flow information;
- multimodal network datasets;
- temporally and geographically aggregated indicators.

Prefer aggregate information when individual traces are unnecessary.

### External model/simulation providers

The eventual public contract must support both:

```text
SmartCities scenario -> adapter -> external execution -> normalized result
```

and:

```text
externally executed model -> result/artifact import -> SmartCities evidence/scenario
```

A deterministic provider comes before commercial adapters.

### External engineering artifacts

SmartCities may reference external geospatial, rail, station, CAD/BIM, photogrammetry, and digital-twin artifacts without absorbing proprietary file formats into the domain.

## Raw sensing versus derived observations

Raw sensing material is not synonymous with urban evidence.

When a source can provide a sufficient derived observation, SmartCities should consume that instead of retaining raw video/images or other high-risk source material.

Biometric identity and face matching are outside the normal mobility integration boundary.

## Supported mechanism policy

An adapter may use the mechanism that best matches the source:

- REST/JSON;
- CSV/file import;
- webhook/event delivery;
- OGC SensorThings API when the source is naturally an IoT observation service;
- OGC API – Features when geospatial feature access is the correct abstraction;
- transport-specific standards such as GTFS/GTFS Realtime where applicable.

Do not wrap every source in the same protocol.

## Contract replay

External integrations use:

```text
Port
  -> Deterministic Fake
  -> Contract Test Suite
  -> Reference Adapter
  -> Replay Fixtures
  -> Capability Discovery
  -> Real Integration E2E
```

Compatibility work uses:

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

## Required adapter documentation

Every executable adapter should eventually document:

- problem/use case;
- SmartCities port;
- supported protocol/product versions;
- prerequisites;
- startup/connectivity;
- configuration;
- authentication/secrets handling;
- health/readiness;
- input mapping;
- output mapping;
- semantic gaps;
- capability discovery;
- timeout/cancellation;
- retry/idempotency semantics;
- error mapping;
- data sensitivity/privacy;
- provenance mapping;
- licensing/deployment obligations;
- fixtures;
- contract-test execution;
- upgrade procedure;
- troubleshooting;
- replacement/custom-adapter tutorial.

## Compatibility states

Never infer support from a product name in a roadmap.

Use one of:

| State | Meaning |
| --- | --- |
| Implemented | Executable adapter exists and has validation evidence. |
| Simulated | Deterministic/synthetic implementation demonstrates the SmartCities contract. |
| Documented | Mapping/instructions exist but no executable compatibility claim is made. |
| Proposed | Candidate integration only. |
| Blocked by external access | More documentation, licensing, credentials, software, or lawful test data are required. |

## Privacy and provenance

Decision-relevant normalized information must preserve applicable source, time, location, method, unit, classification, quality/confidence, transformation, adapter version, dataset/model version, uncertainty, privacy classification, and source reference.

Examples and fixtures in this public repository must be synthetic or legally redistributable.

## Feature and deployment isolation

A deployment without external providers must continue to work.

Provider packages are optional. Integration configuration belongs to the vertical that needs the integration and follows the existing Town Hall feature/configuration authorization model.

## Current implementation status

This document defines architecture and contribution expectations only.

No new generic sensor ingestion runtime, commercial provider adapter, simulation provider, queue, or background processor is implemented by this document.

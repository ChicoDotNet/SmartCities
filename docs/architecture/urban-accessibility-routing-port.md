# Urban Accessibility routing port

## Purpose

V1.5 defines the provider-neutral routing boundary for Urban Accessibility before SmartCities has either a deterministic routing implementation or a real external routing adapter.

The executable contracts live in:

```text
SmartCities.UrbanAccessibility.Routing
```

inside `SmartCities.Core`.

The port expresses normalized SmartCities routing semantics only. It does not implement graph search, transit routing, network preprocessing, coordinate transformation, schedule ingestion, or map rendering.

## Boundary

```text
City Context / application
  -> resolve explicit origin and destination
  -> IAccessibilityRoutingEngine
       -> capability discovery
       -> normalized route query
       -> normalized route result
            -> journeys
            -> legs
            -> provenance
            -> expected failure taxonomy
```

Provider-specific request/response models, authentication, health APIs, diagnostics, and errors remain behind concrete adapters.

## Destination categories do not enter the engine

The citizen product supports:

```text
origin + explicit destination
```

and:

```text
origin + destination category
```

but a destination category is municipal City Context, not routing-engine vocabulary.

Therefore V1.5 routes only explicit points.

A later application layer resolves a destination category to one or more explicit candidate destinations before calling the routing port.

This prevents routing providers from owning municipal taxonomies.

## Route query

`AccessibilityRouteQuery` contains:

```text
requestId
origin
destination
departureAtUtc
allowedModes[]
```

V1.5 modes are intentionally limited to:

```text
Walking
PublicTransport
```

The query:

- requires at least one mode;
- rejects duplicates and unknown enum values;
- requires origin/destination to use the same CRS;
- normalizes departure time to a UTC instant.

The same-CRS rule prevents implicit coordinate transformation inside the core contract.

A later adapter/ingestion boundary can perform an explicit transformation when required.

## Time semantics

The query exposes an absolute UTC departure instant.

A transit adapter is responsible for interpreting that instant against the authoritative source timezone, such as the transit agency/feed timezone.

SmartCities Core does not embed timezone lookup or GTFS schedule logic in V1.5.

Leg departure/arrival times are also normalized to UTC.

Journey total duration is elapsed time from the first leg departure to the final leg arrival, so inter-leg waiting remains visible in the total duration even though waiting is not represented as a movement leg in V1.5.

## Execution timeout and cancellation

Routing semantics and execution policy are separate.

`AccessibilityRoutingExecutionOptions` currently contains:

```text
Timeout > 0
```

The timeout is caller policy passed to the engine implementation.

Implementations/adapters are responsible for enforcing it and mapping an actual execution timeout to:

```text
AccessibilityRoutingFailureCode.TimedOut
```

Caller cancellation uses normal .NET:

```text
CancellationToken
-> OperationCanceledException
```

An adapter must not convert caller cancellation into `TimedOut` or `EngineFailure`.

## Capability discovery

`GetCapabilitiesAsync` returns `AccessibilityRoutingCapabilities` with:

- stable engine ID;
- supported normalized modes;
- scheduled-transit support;
- accessibility-information support;
- path-geometry support.

Capability discovery exists so callers do not infer support from a configured provider name.

An adapter must not advertise a capability it cannot fulfill under its current configuration/data.

V1.5 does not yet define matrix/isochrone/elevation/realtime capability flags because no current observable V1 behavior needs them.

## Journey legs

A `JourneyLeg` is one contiguous movement segment with:

- normalized mode;
- origin/destination;
- UTC departure/arrival;
- distance;
- accessibility state;
- optional provider-neutral path geometry;
- public-transport service reference for transit legs.

Leg distance must use:

```text
Network
or
PathLength
```

semantics.

A straight-line `Geodesic` or `Planar` quantity cannot be normalized as walking/transit route distance.

The optional path must use the same CRS as the leg.

### Transit service reference

Every public-transport leg requires a non-localized provider-neutral service reference.

Examples might eventually map to GTFS route/trip identities, but V1.5 does not prescribe that external representation.

Walking legs cannot carry a transit service reference.

## Transfer semantics

Walking access, transfer walking, and egress are explicit walking legs and do not count as transfers themselves.

For the V1.5 normalized representation:

```text
transfers = max(0, publicTransportLegCount - 1)
```

Therefore adapters must not split one continuous boarding/service movement into multiple `PublicTransport` legs merely because the provider payload contains internal segments.

A new transit leg represents a new continuous boarded service segment.

This keeps transfer counts provider-neutral.

## Journey aggregates

`Journey` derives rather than accepts:

- total duration;
- walking distance;
- transfer count;
- aggregate accessibility.

Walking distance is the sum of walking legs.

All walking legs within one journey must use the same `DistanceSemantics`; otherwise the quantities are not safely additive and the journey is rejected.

Legs may contain waiting gaps but cannot overlap in time.

## Accessibility semantics

V1.5 deliberately does not implement accessibility profiles yet. Those belong to V1.12.

It does define enough normalized state to avoid overclaiming:

```text
Unknown
KnownAccessible
KnownLimited
KnownInaccessible
```

`KnownLimited` requires one or more stable non-localized limitation codes.

`Unknown` cannot carry invented limitations.

Aggregate journey accessibility is conservative:

1. any known-inaccessible leg -> journey known-inaccessible;
2. otherwise any unknown leg -> journey unknown;
3. otherwise any known-limited leg -> journey known-limited;
4. otherwise -> journey known-accessible.

When a journey becomes `Unknown`, SmartCities intentionally does not turn other known-leg information into an overall positive accessibility claim.

## Path geometry

A journey leg may carry a SmartCities `GeoPath` only when the engine advertises path-geometry capability.

V1.5 validates the path CRS but does not:

- snap path endpoints;
- validate topology;
- simplify geometry;
- render maps;
- calculate distance from the path.

Adapters remain responsible for mapping provider geometry into the SmartCities geospatial contract.

## Provenance

Every `AccessibilityRouteResult`, successful or failed, carries `AccessibilityRoutingProvenance`.

It records:

```text
engineId
engineVersion?
adapterId
adapterVersion
sources[]
generatedAtUtc
```

At least one source is required.

Sources reuse `CityContextProvenance`, allowing a result to retain the schedule/network/context versions and effective/retrieval times that materially contributed to routing.

Unknown engine version remains unknown; it is not fabricated.

The adapter version is mandatory because the adapter mapping itself can affect normalized semantics.

## Failure semantics

Expected routing failures are represented without leaking provider-native errors:

| Code | Meaning |
| --- | --- |
| `NoRoute` | No feasible journey was found. |
| `UnsupportedRequest` | Requested normalized semantics/capability is unsupported. |
| `SourceDataUnavailable` | Required schedule/network/context data is unavailable. |
| `SourceDataInvalid` | Required source data is unusable/invalid. |
| `EngineUnavailable` | The routing engine cannot currently be reached/used. |
| `TimedOut` | Execution exceeded the explicit caller timeout. |
| `EngineFailure` | Engine failed without a more specific expected category. |

Provider-native error payloads do not enter the domain result.

Diagnostics and detailed troubleshooting belong to the concrete adapter/operations layer.

Programming defects and caller cancellation are not expected routing failures.

## Success/failure result invariant

`AccessibilityRouteResult` is exclusive:

Successful:

```text
journeys: one or more
failure: null
provenance: required
```

Failed:

```text
journeys: empty
failure: required
provenance: required
```

Journey identifiers must be unique inside one result.

## Privacy

Origin/destination pairs can reveal movement intent.

V1.5 authorizes no persistence, telemetry, analytics, or logging of route queries.

Later API/observability increments must make an explicit privacy decision before retaining coordinates, destination intent, accessibility needs, or journey history.

Demo Town remains the only public routing-fixture context until lawfully redistributable alternatives are introduced.

## Contract replay direction

V1.5 establishes the public port.

The expected sequence remains:

```text
IAccessibilityRoutingEngine
  -> V1.6 deterministic routing implementation
  -> reusable routing contract suite
  -> V1.9 real reference adapter
  -> replay fixtures
  -> capability discovery
  -> real integration E2E
```

The public contract must not change merely to mirror the first real provider.

## Non-goals

V1.5 does not implement:

- routing algorithms;
- deterministic routing behavior;
- OpenTripPlanner or another provider adapter;
- GTFS ingestion;
- pedestrian-network ingestion;
- routing graph/topology;
- destination-category resolution;
- matrix routing;
- isochrones;
- realtime transit;
- fare calculation;
- cycling;
- route API/UI;
- map rendering;
- persistence;
- health/readiness endpoints.

## Acceptance evidence

V1.5 is complete when tests prove:

- route-query validation and UTC time semantics;
- explicit execution timeout;
- capability discovery contract;
- provider-neutral journey modes;
- walking/transit leg semantics;
- transfer rules;
- distance semantics;
- path CRS validation;
- accessibility known/unknown/limited/inaccessible behavior;
- conservative journey accessibility aggregation;
- provenance and defensive copies;
- success/failure exclusivity;
- expected failure taxonomy;
- caller cancellation token availability on the public port;

and exact-head CI is green.

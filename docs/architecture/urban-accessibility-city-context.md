# Urban Accessibility City Context v1

## Purpose

V1.3 introduces the smallest provider-neutral city-context model required by Urban Accessibility.

City Context answers:

- which destination categories exist;
- which destinations/points of interest belong to those categories;
- which municipal/analysis zones exist;
- which pedestrian and public-transport network datasets are available;
- which public-transport stops belong to which transit network;
- where each record came from and how current it may be.

This increment defines immutable domain contracts only.

It does not acquire, persist, route, render, or spatially analyze City Context data.

## Executable contracts

The contracts live in:

```text
SmartCities.CityContext
```

inside `SmartCities.Core`.

| Contract | Responsibility |
| --- | --- |
| `CityContextProvenance` | Source identity, optional source version/effective time, and retrieval time. |
| `DestinationCategory` | Stable non-localized destination-category key. |
| `CityZone` | Municipal/analysis zone using the V1.2 external polygon reference. |
| `PointOfInterest` | Destination ID + category + point + provenance. |
| `MobilityNetworkKind` | Minimal V1 network families: pedestrian and public transport. |
| `MobilityNetworkReference` | Provider-neutral external network reference + provenance. |
| `TransitStopReference` | Provider-neutral stop reference + network ID + point + provenance. |
| `CityContextSnapshot` | Immutable catalogs plus referential-integrity checks. |

## Provenance

City Context data is not automatically Evidence Hub evidence.

A zone, POI, stop, or network can later contribute to evidence, routing, analysis, or explanation, but V1.3 preserves its source identity first.

`CityContextProvenance` records:

```text
sourceSystem
sourceReference
sourceVersion?     // unknown is allowed
effectiveAtUtc?    // unknown freshness is allowed
retrievedAtUtc     // always known to the caller constructing the context
```

Unknown source version or effective time remains `null`.

SmartCities must not fabricate a version or effective date merely to make freshness appear complete.

All supplied timestamps are normalized to UTC.

The distinction is deliberate:

```text
effectiveAtUtc
  = when the source says the data applies

retrievedAtUtc
  = when SmartCities received/retrieved it
```

Retrieval time does not prove freshness.

## Destination categories

A `DestinationCategory` contains a stable canonical identifier such as:

```text
healthcare
education
public-services
employment
```

These examples are not a frozen taxonomy.

V1.3 does not ship a global category ontology.

A municipality/Demo Town can define the categories required by the product context.

The category identifier is non-localized product data.

Citizen/operator display names belong to the localization layer when UI is introduced.

## Zones

A `CityZone` combines:

```text
ZonePolygonReference
+ CityContextProvenance
```

The polygon remains external.

V1.3 does not embed polygon coordinates or introduce a GIS geometry library.

The zone ID remains the ID already owned by `ZonePolygonReference`; City Context does not create a second competing zone identity.

## Points of interest / destinations

A `PointOfInterest` contains:

```text
pointOfInterestId
categoryId
GeoPoint
provenance
```

A POI is a destination known to City Context.

It does not imply:

- commercial business metadata;
- opening hours;
- routing reachability;
- accessibility;
- service quality;
- capacity;
- citizen endorsement.

Those semantics require separate evidence/contracts when a vertical needs them.

## Mobility network references

V1.3 introduces only two network kinds because they are the only ones V1 currently requires:

```text
Pedestrian
PublicTransport
```

A `MobilityNetworkReference` identifies:

```text
networkId
networkKind
external networkReference
provenance
```

The external reference may later resolve to a Demo Town resource, GTFS-derived network, pedestrian graph, routing-engine dataset, OGC resource, or another provider-neutral integration.

The reference is not a network graph.

No nodes, edges, topology, cost functions, transfer rules, or routing behavior are introduced here.

## Transit stops

A `TransitStopReference` contains:

```text
stopId
networkId
external stopReference
GeoPoint
provenance
```

It deliberately does not use GTFS DTOs.

V1.7 may map GTFS stops into this City Context contract, but GTFS remains an integration/input format rather than the domain model.

## Snapshot and referential integrity

`CityContextSnapshot` defensively copies all supplied catalogs and rejects:

- duplicate destination-category IDs;
- duplicate zone IDs;
- duplicate POI IDs;
- duplicate network IDs;
- duplicate stop IDs;
- POIs whose category is absent;
- stops whose network is absent;
- transit stops attached to a non-public-transport network.

These checks establish only internal catalog consistency.

The snapshot does not prove:

- that geometry references can be resolved;
- that a POI lies inside a zone;
- that a stop is spatially connected to a network;
- that a pedestrian network reaches a stop;
- that a transit network contains a route serving the stop;
- that data from different sources use the same CRS;
- that a source is current or accurate.

Those claims require later observable behavior and evidence.

## CRS policy

City Context reuses V1.2 geospatial primitives.

Different records may legally reference different CRSs.

V1.3 does not silently transform them into a common CRS.

A later ingestion/adapter/routing boundary must make any required transformation explicit.

## Privacy

City Context should contain public-safe or appropriately governed municipal context.

V1.3 does not introduce:

- citizen origin/destination histories;
- person-level mobility traces;
- biometrics;
- private address books;
- commercial user profiles.

POI/zone/network fixtures in the public repository must remain synthetic or lawfully redistributable.

## Relationship to upcoming increments

### V1.4 — Demo Town

V1.4 provides the reproducible synthetic [Demo Town fixture](../../samples/demo-town/README.md):

- destination categories;
- zones backed by GeoJSON polygon references;
- POIs backed by GeoJSON points;
- a pedestrian GeoJSON line network;
- a public-transport network backed by a tiny GTFS Schedule fixture;
- transit stops mapped from GTFS into these provider-neutral contracts.

The sample test suite reconstructs `CityContextSnapshot` from the distributed artifacts so the sample cannot silently drift away from the domain contracts.

This does not make GeoJSON or GTFS the City Context domain model.

### V1.5 — Routing port

The routing port can consume City Context references without owning them and without exposing provider-specific models.

### V1.7 — GTFS Schedule ingestion

GTFS ingestion can map standards-based schedule/stop information into City Context and routing inputs without redefining the domain.

## Non-goals

V1.3 does not implement:

- City Context persistence;
- import/refresh jobs;
- REST APIs;
- UI;
- GIS rendering;
- polygon geometry;
- spatial joins;
- point-in-polygon;
- coordinate transformation;
- geocoding;
- GTFS ingestion;
- realtime transit;
- routing;
- pathfinding;
- network topology;
- destination search/ranking;
- municipal taxonomy governance.

## V1.3 acceptance evidence

The increment is complete when tests prove:

- source/version/effective/retrieval provenance semantics;
- unknown version/freshness stays unknown;
- stable destination-category IDs;
- zone reuse of V1.2 polygon references;
- provider-neutral POI/stop/network records;
- immutable catalog snapshots;
- unique identifiers;
- POI → category integrity;
- stop → public-transport-network integrity;

and exact-head CI is green.

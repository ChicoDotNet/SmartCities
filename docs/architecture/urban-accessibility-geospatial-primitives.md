# Urban Accessibility geospatial primitives

## Purpose

V1.2 introduces the smallest geospatial vocabulary required by Urban Accessibility without introducing a GIS platform.

The executable contracts live in:

```text
SmartCities.Geospatial
```

and remain part of `SmartCities.Core`.

They provide geometry/value semantics only. They do not route, project, render, persist, geocode, simplify, intersect, buffer, or calculate spatial relationships.

## Public primitives

| Contract | Purpose | Deliberate limitation |
| --- | --- | --- |
| `CoordinateReferenceSystem` | Provider-neutral authority/code identity for a CRS. | No projection/transform engine. |
| `GeoPoint` | One 2D coordinate in an explicit CRS. | No altitude/elevation dimension in V1.2. |
| `GeoBoundingBox` | Ordered axis-aligned extent in one CRS. | No antimeridian wrapping semantics. |
| `GeoPath` | Ordered line/path points in one CRS. | No topology, simplification, routing, or length calculation. |
| `ZonePolygonReference` | Stable zone + externally owned polygon reference + expected CRS. | Does not embed polygon geometry. |
| `Distance` | Non-negative distance quantity in canonical meters. | Does not calculate a distance. |
| `DistanceSemantics` | Declares how a distance was derived. | Does not select or implement an algorithm. |

## Coordinate reference systems

A CRS is identified as:

```text
AUTHORITY:CODE
```

Examples:

```text
EPSG:4326
EPSG:32613
OGC:CRS84
```

The authority is normalized to upper case. The code is retained as authority-local text.

`CoordinateReferenceSystem.Epsg(code)` is a convenience factory for positive EPSG codes.

`CoordinateReferenceSystem.Wgs84` is the canonical SmartCities value for:

```text
EPSG:4326
```

This contract is identification metadata only. SmartCities Core does not transform coordinates between CRSs.

Transformations belong behind a later integration/application boundary when a concrete vertical requires them.

## Canonical coordinate ordering

SmartCities uses:

```text
X, Y
```

for its public point contract.

For `EPSG:4326`, SmartCities explicitly interprets:

```text
X = longitude
Y = latitude
```

This ordering is a SmartCities contract and is independent of how a provider or specification serializes axes.

An adapter that receives latitude/longitude order, formal CRS axis order, GeoJSON order, a projected grid, or another provider convention must map that representation explicitly into the SmartCities X/Y contract.

This rule exists to prevent silent coordinate swaps.

## Coordinate validation

All coordinates must be finite numbers.

For `EPSG:4326`, SmartCities additionally validates:

```text
-180 <= longitude <= 180
 -90 <= latitude  <= 90
```

For another CRS, V1.2 validates finiteness only because SmartCities Core does not contain the authoritative axis/domain rules for every CRS.

A future adapter or City Context import may apply stricter CRS-specific validation without changing this primitive.

## Bounding-box semantics

A `GeoBoundingBox` uses:

```text
MinX <= MaxX
MinY <= MaxY
```

and one explicit CRS.

For WGS 84, both corners must also satisfy longitude/latitude validation.

A WGS 84 box that crosses the antimeridian would require wrapped-longitude semantics such as `MinX > MaxX`. V1.2 deliberately rejects that representation rather than making a hidden geographic assumption.

If a future real use case requires antimeridian-spanning areas, introduce that behavior through an observable contract and tests.

## Path semantics

A `GeoPath`:

- contains at least two points;
- preserves caller order;
- defensively snapshots the point collection;
- requires every point to use the same CRS.

It is a geometry supplied by another component.

It does not imply:

- a routable network;
- a valid road/transit path;
- topology;
- an isochrone;
- a polygon boundary;
- a calculated distance.

## Zone/polygon reference

`ZonePolygonReference` deliberately does not implement polygon geometry.

It preserves:

```text
zone ID
+ polygon reference
+ expected CRS
```

The polygon reference may later resolve through City Context, a municipal dataset, OGC-facing collection, or another provider-neutral source.

Provider/GIS-native geometry objects must not become SmartCities domain contracts.

V1.3 City Context will decide how zones and geometry references are cataloged and sourced.

## Distance semantics

`Distance` stores meters as the canonical unit and exposes kilometers as a derived convenience value.

Every distance requires one explicit `DistanceSemantics` value:

| Semantics | Meaning |
| --- | --- |
| `Geodesic` | Shortest surface distance on an earth/geodetic model. |
| `Planar` | Straight-line distance in a projected/cartesian plane. |
| `PathLength` | Length along a supplied path geometry. |
| `Network` | Distance along a routable network/journey. |

These values describe provenance of the quantity; they are not algorithms.

For example:

```text
1,200 m Network
```

does not claim that SmartCities Core calculated the route. A routing adapter can provide that quantity later.

This distinction prevents a straight-line distance from silently being presented as walking distance.

## Privacy

These primitives authorize no persistence or telemetry by themselves.

Origin/destination pairs can reveal movement intent. Later APIs must independently decide what may be stored or logged.

No real citizen coordinates belong in public fixtures.

## Dependency rule

V1.2 adds no GIS package or spatial database dependency.

The current direction remains:

```text
SmartCities product/domain contracts
  -> small SmartCities-owned geospatial values
  -> later provider-neutral ports
  -> external GIS/routing engines when required
```

## Non-goals

V1.2 does not implement:

- map rendering or tiles;
- spatial database columns/indexes;
- geometry libraries;
- polygon coordinates/topology;
- coordinate transforms/projections;
- geocoding/reverse geocoding;
- routing;
- network graphs;
- GTFS;
- isochrones;
- spatial joins;
- point-in-polygon;
- distance calculation;
- area calculation.

## V1.2 acceptance evidence

The increment is complete when tests prove:

- CRS identity/normalization;
- finite coordinates;
- WGS 84 longitude/latitude bounds;
- ordered bbox extents;
- path minimum size, order, defensive copy, and same-CRS rule;
- zone/polygon reference validation;
- canonical meter storage;
- explicit distance semantics;
- invalid/negative/non-finite distance rejection;

and exact-head CI is green.

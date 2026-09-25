# SmartCities Demo Town v1

Demo Town is a deliberately small, fully synthetic municipality used to exercise SmartCities contracts without requiring confidential municipal data, citizen PII, commercial datasets, or proprietary software.

Nothing in this directory represents a real municipality, transit agency, person, address, service level, or operational promise.

All files are original synthetic project fixtures and are distributed under the repository's AGPL-3.0-only license.

## What V1.4 contains

```text
samples/demo-town/
  city-context.json
  geography/
    boundary.geojson
    zones.geojson
    pois.geojson
  networks/
    pedestrian.geojson
  transit/
    gtfs/
      agency.txt
      stops.txt
      routes.txt
      trips.txt
      stop_times.txt
      calendar.txt
      shapes.txt
      feed_info.txt
```

The sample contains:

- one synthetic municipal boundary;
- three synthetic analysis zones;
- three destination categories;
- four synthetic points of interest;
- a tiny pedestrian line network;
- three transit stops;
- one bus route;
- two scheduled trips;
- a minimal route shape in each direction.

## Coordinate convention

All sample geometry uses:

```text
EPSG:4326
SmartCities X,Y convention
X = longitude
Y = latitude
```

Coordinates are illustrative synthetic geometry only. They are not intended to reproduce or approximate an actual municipal boundary or transport system.

## City Context mapping

`city-context.json` is a Demo Town manifest, not a replacement for a transport or GIS standard.

It provides stable sample identity, provenance, destination categories, and provider-neutral references to the pedestrian and transit datasets.

The executable tests map:

- zones from `geography/zones.geojson`;
- POIs from `geography/pois.geojson`;
- network references from `city-context.json`;
- transit stops from the GTFS `stops.txt`;

into the V1.3 `CityContextSnapshot`.

GeoJSON remains an external sample representation; GeoJSON types do not enter the SmartCities domain.

## Transit data

The transit fixture follows GTFS Schedule rather than inventing a SmartCities transit format.

V1.4 does **not** implement production GTFS ingestion. The test suite performs only bounded fixture consistency checks.

Production acquisition, validation, diagnostics, freshness handling, and mapping belong to V1.7.

The fixture includes `calendar.txt`, so `calendar_dates.txt` is not required for this regular synthetic service pattern.

## Validate the sample

From the repository root:

```bash
dotnet test tests/SmartCities.Core.Tests/SmartCities.Core.Tests.csproj \
  --filter FullyQualifiedName~DemoTownSampleTests
```

The tests prove that:

1. every documented sample resource is present;
2. GeoJSON artifacts have the expected bounded geometry families;
3. the data maps to a valid V1.3 `CityContextSnapshot`;
4. category/network/stop references remain internally consistent;
5. the tiny GTFS schedule has coherent agency/route/trip/service/stop/shape references.

## What this does not prove

A green Demo Town fixture test does not prove:

- routing works;
- pedestrian links are topologically connected;
- a stop is reachable from a POI;
- transit service is realistic;
- accessibility is known;
- geometry is suitable for a real municipality;
- GTFS passes every rule in the complete upstream specification;
- a map UI exists.

Those capabilities belong to later V1 increments.

## Replace with municipal data

V1.4 is intentionally a reference fixture, not an ingestion framework.

As later increments arrive, the intended progression is:

```text
run Demo Town
-> understand the contracts
-> replace synthetic context with municipal data
-> validate imports
-> connect a routing engine
-> run contract tests
-> exercise citizen and official outcomes
```

Do not place confidential municipal datasets, citizen PII, identifiable mobility traces, or non-redistributable provider data in this public directory.

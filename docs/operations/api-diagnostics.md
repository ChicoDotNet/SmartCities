# API diagnostics

SmartCities exposes a small deployment-friendly diagnostics surface.

## OpenAPI

```text
GET /openapi/v1.json
```

The document is generated from the ASP.NET Core endpoint metadata and includes the public citizen-mobility and localization APIs. It is intended for tooling, client generation, contract inspection, and future API documentation surfaces.

No interactive Swagger UI is part of this increment.

## Liveness

```text
GET /health/live
```

Liveness answers one question: **is the API process alive and able to serve requests?**

It deliberately runs no dependency checks. A database outage therefore does not make the process liveness endpoint unhealthy.

Example:

```json
{
  "status": "Healthy",
  "checks": {}
}
```

## Readiness

```text
GET /health/ready
```

Readiness answers whether the API is ready to perform application work. The current readiness contract includes relational database reachability through the configured `SmartCitiesDbContext`.

Example:

```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy"
  }
}
```

When the database cannot be reached, readiness returns HTTP 503 while liveness can remain HTTP 200.

## Verification

The `Local Vertical Slice CI` workflow starts a real PostgreSQL service and verifies liveness, database readiness, OpenAPI generation, report creation, idempotent replay, and persisted case recovery over HTTP.

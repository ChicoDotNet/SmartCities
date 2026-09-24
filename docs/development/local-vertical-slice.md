# Local executable vertical slice

The first citizen-mobility path can run locally against PostgreSQL without introducing a development-only domain persistence provider.

A separate SQLite file stores only non-sensitive deployment configuration such as per-Town-Hall feature flags. It is not used for citizen or decision data.

## 1. Start PostgreSQL

From the repository root:

```bash
docker compose up -d database
```

The Compose profile binds PostgreSQL only to loopback on port `54329`. The committed username/password are development-only values and are not production secrets.

## 2. Start the API

```bash
dotnet run --project src/SmartCities.Api/SmartCities.Api.csproj
```

The Development profile:

- listens on `http://localhost:5000`;
- identifies the deployment as `local-town-hall`;
- uses `smartcities.configuration.db` for non-sensitive feature/configuration state;
- selects PostgreSQL for domain persistence;
- connects to the Compose database;
- applies the PostgreSQL EF Core migration chain on startup.

Automatic migration is gated by both the `Development` environment and `SmartCities:Persistence:ApplyMigrationsOnStartup=true`. Production startup never automatically opts into this development behavior.

## 3. Start the React client

In another terminal:

```bash
cd src/SmartCities.Web
npm ci
npm run dev
```

Vite proxies `/api` to `http://localhost:5000` by default.

A browser submission therefore travels through:

```text
SmartCities.Web
    ↓
SmartCities.Api
    ↓
CitizenMobilityReportsController
    ↓
ICitizenMobilityReportService
    ↓
ICitizenMobilityReportRepository
    ↓
SmartCitiesDbContext
    ↓
PostgreSQL
```

## Recovery checks

After a report is accepted, the authoritative persisted case can be retrieved with:

```text
GET /api/citizen/mobility-reports/{reportId}
```

Its citizen-safe reviewed state is available through:

```text
GET /api/citizen/mobility-reports/{reportId}/outcome
```

The outcome endpoint reads persisted report and human-review state rather than reconstructing it from browser memory. React carries the opaque report reference in the URL so the same outcome can be recovered after refresh or direct navigation.

## Feature-flag check

The effective feature snapshot is public non-sensitive deployment metadata:

```text
GET /api/system/features
```

The `citizen-mobility` slice defaults to enabled. An authenticated Town Hall administrator with the canonical `feature-flags.manage` permission can set an override through:

```text
PUT /api/system/features/citizen-mobility
{ "enabled": false }
```

When disabled, citizen mobility routes return 404 and React does not render that vertical slice.

## Automated verification

`Local Vertical Slice CI` boots a real PostgreSQL service for domain state and a separate SQLite file for non-sensitive control-plane state. It proves the default feature snapshot, persists a disabled override, verifies the mobility route becomes 404, re-enables the feature, then executes the complete F3 flow.

The gate also proves that post-finalization report replay cannot overwrite human authority, changing locale cannot change report/case/status/disposition identity, and an unsupported locale falls back to neutral English.

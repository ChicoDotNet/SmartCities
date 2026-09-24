# Local executable vertical slice

The first citizen-mobility path can run locally against PostgreSQL without introducing a development-only persistence provider.

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
- selects PostgreSQL;
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

## Automated verification

`Local Vertical Slice CI` boots a real PostgreSQL service, starts the API in Development mode, applies migrations, creates and replays a citizen report, recovers its authoritative case, reads the pending citizen outcome, finalizes the persisted review through canonical JWT authorization, and reads the localized final citizen outcome.

The gate also proves that post-finalization report replay cannot overwrite human authority, changing locale cannot change report/case/status/disposition identity, and an unsupported locale falls back to neutral English.

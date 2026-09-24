# Local executable vertical slice

The first citizen-mobility path can run locally against PostgreSQL without introducing a development-only domain persistence provider.

A separate SQLite control-plane file stores non-sensitive deployment settings such as Feature Flags plus typed Administration whitelist metadata. It is not used for citizen or decision data. Exact staff email rules are access-control/PII metadata and live in their own table rather than the generic non-sensitive settings table.

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
- uses `smartcities.configuration.db` for local control-plane state;
- exposes the development-only bootstrap login `townhalladmin@smartcities.local` with the committed Development-only password `smartcities-local-bootstrap-only`;
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

Feature mutation also requires current Town Hall Administration admission.

## Administration bootstrap and whitelist

With an empty local whitelist, the Development bootstrap account can create the first Administration rule:

```text
POST /api/administration/bootstrap/session
userName = townhalladmin@smartcities.local
password = smartcities-local-bootstrap-only
```

That password exists only in `appsettings.Development.json` for local development. Production must supply `SmartCities:Administration:Bootstrap:Password` through its secret configuration mechanism; there is no production default.

After the first whitelist rule is persisted, the bootstrap identity immediately loses Administration admission even if its encrypted browser cookie has not expired.

Portable rule kinds are `email-domain`, `email`, and `canonical-subject`.

## Automated verification

`Local Vertical Slice CI` boots a real PostgreSQL service for domain state and a separate SQLite file for control-plane state. It proves empty-whitelist bootstrap login, creation of the first whitelist rule, immediate revocation of the existing bootstrap session for Administration operations, admission of a whitelisted canonical subject, Feature Flag disable/re-enable, and then the complete F3 flow.

The gate also proves that post-finalization report replay cannot overwrite human authority, changing locale cannot change report/case/status/disposition identity, and an unsupported locale falls back to neutral English.

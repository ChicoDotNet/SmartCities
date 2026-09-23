# Build identity and OpenTelemetry

SmartCities exposes public deployment identity and registers OpenTelemetry without selecting a monitoring vendor.

## Build metadata

```text
GET /api/system/build
```

The response contains only public deployment identity:

- `serviceName`;
- `version`;
- `informationalVersion`;
- optional `commitSha`;
- optional `buildId`.

Deployment pipelines can provide source/build identity with:

```text
SmartCities__Build__CommitSha
SmartCities__Build__BuildId
```

When no commit is configured, the API may recover a hexadecimal source revision from the assembly informational-version suffix when the SDK embedded one.

No environment name, host name, connection string, secret, or user information is exposed.

## OpenTelemetry tracing

The API registers OpenTelemetry ASP.NET Core tracing even when no exporter is configured.

The resource uses:

```text
service.name    = SmartCities.Api
service.version = <running assembly version>
```

When build metadata is present, SmartCities also adds vendor-neutral custom resource attributes:

```text
smartcities.build.commit
smartcities.build.id
```

The existing request middleware continues to attach `smartcities.correlation_id` to the current ASP.NET Core Activity.

## Optional OTLP export

No collector is required for local development or normal application startup.

OTLP export becomes active only when either of these configuration values is present:

```text
SmartCities__Observability__Otlp__Endpoint
OTEL_EXPORTER_OTLP_ENDPOINT
```

The SmartCities-specific key takes precedence. The endpoint must be an absolute HTTP or HTTPS URI.

OTLP is the provider-neutral protocol seam. A deployment may point it at an OpenTelemetry Collector or any compatible backend without changing application or domain code.

## Deliberate boundary

This increment exports traces only. Structured application logs continue through `Microsoft.Extensions.Logging`, and metrics remain a future opt-in addition. This keeps the first OpenTelemetry slice small while preserving a standard path for later traces, logs, and metrics backends.

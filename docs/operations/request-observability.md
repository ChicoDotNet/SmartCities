# Request observability

SmartCities establishes a vendor-neutral request correlation boundary before authentication and authorization providers are introduced.

## Correlation

Every HTTP response includes:

```text
X-Correlation-ID
```

A caller may provide one correlation ID using the same request header. The API preserves it only when it is a single value, 64 characters or fewer, and contains ASCII letters, digits, `-`, `_`, `.`, or `:`.

Unsafe, malformed, multiple, or oversized values do not reject the citizen request. They are replaced with a server-generated identifier.

Correlation IDs are support identifiers. They are intentionally distinct from W3C trace IDs.

## Trace context

ASP.NET Core's current W3C `Activity` remains the distributed-tracing identity. Request logs include both:

- `CorrelationId`
- `TraceId`

The correlation ID is also attached to the current Activity as the `smartcities.correlation_id` tag so a future OpenTelemetry exporter can carry it without changing application contracts.

No telemetry vendor or collector is required by this increment.

## Structured request completion

Each request produces a structured completion event containing:

- request method;
- request path;
- status code;
- elapsed milliseconds;
- correlation ID;
- W3C trace ID.

The correlation/trace identifiers also live in the request logging scope so downstream logs can inherit the same operational context when the configured sink supports scopes.

## Privacy boundary

The request observability middleware deliberately does **not** log:

- query strings;
- request or response bodies;
- authorization headers;
- cookies;
- citizen evidence payloads;
- connection strings.

This keeps the default operational trail useful without turning request telemetry into a second citizen-data store.

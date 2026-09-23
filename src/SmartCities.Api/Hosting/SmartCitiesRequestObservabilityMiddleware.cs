using System.Diagnostics;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Establishes a privacy-safe correlation boundary and structured completion log for each HTTP request.
/// </summary>
public sealed class SmartCitiesRequestObservabilityMiddleware
{
  /// <summary>Public request/response header used for support correlation.</summary>
  public const string CorrelationHeaderName =
    "X-Correlation-ID";

  private const int MaxCorrelationIdLength = 64;

  private readonly RequestDelegate next;
  private readonly ILogger<SmartCitiesRequestObservabilityMiddleware> logger;

  /// <summary>Initializes the request observability middleware.</summary>
  /// <param name="next">Next middleware in the HTTP pipeline.</param>
  /// <param name="logger">Structured logger for request completion events.</param>
  public SmartCitiesRequestObservabilityMiddleware(
    RequestDelegate next,
    ILogger<SmartCitiesRequestObservabilityMiddleware> logger)
  {
    ArgumentNullException.ThrowIfNull(next);
    ArgumentNullException.ThrowIfNull(logger);

    this.next = next;
    this.logger = logger;
  }

  /// <summary>Applies correlation and structured request logging to the current HTTP request.</summary>
  /// <param name="context">Current HTTP context.</param>
  /// <returns>A task representing the request pipeline.</returns>
  public async Task InvokeAsync(
    HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var correlationId = ResolveCorrelationId(context);
    var activity = Activity.Current;
    var traceId = activity?.TraceId.ToString()
      ?? context.TraceIdentifier;
    var spanId = activity?.SpanId.ToString()
      ?? string.Empty;

    activity?.SetTag(
      "smartcities.correlation_id",
      correlationId);

    context.Response.Headers[CorrelationHeaderName] =
      correlationId;

    using var scope = logger.BeginScope(
      new Dictionary<string, object?>(StringComparer.Ordinal)
      {
        ["CorrelationId"] = correlationId,
        ["TraceId"] = traceId,
        ["SpanId"] = spanId,
        ["RequestId"] = context.TraceIdentifier,
      });

    var startedAt = Stopwatch.GetTimestamp();

    try
    {
      await next(context).ConfigureAwait(false);

      if (logger.IsEnabled(LogLevel.Information))
      {
        RequestObservabilityLog.RequestCompleted(
          logger,
          context.Request.Method,
          context.Request.Path.Value ?? "/",
          context.Response.StatusCode,
          Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
          correlationId,
          traceId);
      }
    }
    catch
    {
      if (logger.IsEnabled(LogLevel.Warning))
      {
        RequestObservabilityLog.RequestFailed(
          logger,
          context.Request.Method,
          context.Request.Path.Value ?? "/",
          Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
          correlationId,
          traceId);
      }

      throw;
    }
  }

  private static string ResolveCorrelationId(
    HttpContext context)
  {
    var headerValues =
      context.Request.Headers[CorrelationHeaderName];

    if (headerValues.Count == 1
      && IsSafeCorrelationId(headerValues[0]))
    {
      return headerValues[0]!;
    }

    var activityTraceId =
      Activity.Current?.TraceId.ToString();

    return string.IsNullOrWhiteSpace(activityTraceId)
      ? Guid.NewGuid().ToString("N")
      : activityTraceId;
  }

  private static bool IsSafeCorrelationId(
    string? candidate)
  {
    if (string.IsNullOrWhiteSpace(candidate)
      || candidate.Length > MaxCorrelationIdLength)
    {
      return false;
    }

    foreach (var character in candidate)
    {
      if (!char.IsAsciiLetterOrDigit(character)
        && character is not '-' and not '_' and not '.' and not ':')
      {
        return false;
      }
    }

    return true;
  }
}

internal static partial class RequestObservabilityLog
{
  [LoggerMessage(
    EventId = 1100,
    EventName = "RequestCompleted",
    Level = LogLevel.Information,
    Message = "HTTP {RequestMethod} {RequestPath} completed with {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId={CorrelationId} TraceId={TraceId}")]
  internal static partial void RequestCompleted(
    ILogger logger,
    string requestMethod,
    string requestPath,
    int statusCode,
    double elapsedMilliseconds,
    string correlationId,
    string traceId);

  [LoggerMessage(
    EventId = 1101,
    EventName = "RequestFailed",
    Level = LogLevel.Warning,
    Message = "HTTP {RequestMethod} {RequestPath} failed after {ElapsedMilliseconds} ms. CorrelationId={CorrelationId} TraceId={TraceId}")]
  internal static partial void RequestFailed(
    ILogger logger,
    string requestMethod,
    string requestPath,
    double elapsedMilliseconds,
    string correlationId,
    string traceId);
}

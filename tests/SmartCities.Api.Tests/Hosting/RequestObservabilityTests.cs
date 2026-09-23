using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCities.Api.Hosting;
using Xunit;

namespace SmartCities.Api.Tests.Hosting;

public sealed class RequestObservabilityTests
{
  [Fact]
  public async Task Missing_correlation_id_is_generated_and_returned_to_the_client()
  {
    var logs = new RecordingLoggerProvider();
    await using var app = await StartAsync(logs);
    using var client = app.GetTestClient();

    using var response = await client.GetAsync(
      "/probe",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var correlationId = Assert.Single(
      response.Headers.GetValues("X-Correlation-ID"));

    Assert.InRange(correlationId.Length, 16, 64);
    Assert.All(
      correlationId,
      character => Assert.True(
        char.IsAsciiLetterOrDigit(character)
        || character is '-' or '_' or '.' or ':'));
  }

  [Fact]
  public async Task Valid_client_correlation_id_is_preserved()
  {
    var logs = new RecordingLoggerProvider();
    await using var app = await StartAsync(logs);
    using var client = app.GetTestClient();
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      "/probe");

    request.Headers.Add(
      "X-Correlation-ID",
      "citizen-request-123");

    using var response = await client.SendAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(
      "citizen-request-123",
      Assert.Single(
        response.Headers.GetValues("X-Correlation-ID")));
  }

  [Fact]
  public async Task Unsafe_client_correlation_id_is_replaced_without_rejecting_the_request()
  {
    var logs = new RecordingLoggerProvider();
    await using var app = await StartAsync(logs);
    using var client = app.GetTestClient();
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      "/probe");

    request.Headers.Add(
      "X-Correlation-ID",
      "unsafe correlation value");

    using var response = await client.SendAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var correlationId = Assert.Single(
      response.Headers.GetValues("X-Correlation-ID"));

    Assert.NotEqual(
      "unsafe correlation value",
      correlationId);
  }

  [Fact]
  public async Task Completion_log_contains_structured_safe_request_metadata()
  {
    var logs = new RecordingLoggerProvider();
    await using var app = await StartAsync(logs);
    using var client = app.GetTestClient();
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      "/probe?citizenSecret=must-not-be-logged");

    request.Headers.Add(
      "X-Correlation-ID",
      "support-ticket-42");

    using var response = await client.SendAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var entry = Assert.Single(
      logs.Entries,
      item =>
        item.Category
          == "SmartCities.Api.Hosting.SmartCitiesRequestObservabilityMiddleware"
        && item.EventId.Name == "RequestCompleted");

    Assert.Equal(
      "support-ticket-42",
      entry.State["CorrelationId"]);
    Assert.Equal(
      "GET",
      entry.State["RequestMethod"]);
    Assert.Equal(
      "/probe",
      entry.State["RequestPath"]);
    Assert.Equal(
      200,
      entry.State["StatusCode"]);

    var traceId = Assert.IsType<string>(
      entry.State["TraceId"]);
    Assert.NotEmpty(traceId);

    Assert.DoesNotContain(
      "citizenSecret",
      entry.Message,
      StringComparison.Ordinal);
    Assert.DoesNotContain(
      "must-not-be-logged",
      entry.Message,
      StringComparison.Ordinal);
    Assert.DoesNotContain(
      entry.State.Keys,
      key => string.Equals(
        key,
        "QueryString",
        StringComparison.Ordinal));
    Assert.DoesNotContain(
      entry.State.Keys,
      key => string.Equals(
        key,
        "RequestBody",
        StringComparison.Ordinal));
  }

  private static async Task<WebApplication> StartAsync(
    RecordingLoggerProvider logs)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(logs);
    builder.Services.AddSmartCitiesApiObservability();

    var app = builder.Build();

    app.UseSmartCitiesRequestObservability();
    app.MapGet(
      "/probe",
      static () => Results.Ok());

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    return app;
  }

  private sealed class RecordingLoggerProvider : ILoggerProvider
  {
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    public ILogger CreateLogger(string categoryName) =>
      new RecordingLogger(
        categoryName,
        Entries);

    public void Dispose()
    {
    }
  }

  private sealed class RecordingLogger : ILogger
  {
    private readonly string category;
    private readonly ConcurrentQueue<LogEntry> entries;

    public RecordingLogger(
      string category,
      ConcurrentQueue<LogEntry> entries)
    {
      this.category = category;
      this.entries = entries;
    }

    public IDisposable? BeginScope<TState>(
      TState state)
      where TState : notnull =>
      NullScope.Instance;

    public bool IsEnabled(
      LogLevel logLevel) =>
      true;

    public void Log<TState>(
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter)
    {
      var properties =
        state as IEnumerable<KeyValuePair<string, object?>>
        ?? [];

      entries.Enqueue(
        new LogEntry(
          category,
          logLevel,
          eventId,
          properties.ToDictionary(
            static pair => pair.Key,
            static pair => pair.Value,
            StringComparer.Ordinal),
          formatter(state, exception)));
    }
  }

  private sealed record LogEntry(
    string Category,
    LogLevel Level,
    EventId EventId,
    IReadOnlyDictionary<string, object?> State,
    string Message);

  private sealed class NullScope : IDisposable
  {
    public static NullScope Instance { get; } = new();

    public void Dispose()
    {
    }
  }
}

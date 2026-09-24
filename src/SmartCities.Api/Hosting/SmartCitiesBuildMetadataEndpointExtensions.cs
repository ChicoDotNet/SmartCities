namespace SmartCities.Api.Hosting;

/// <summary>
/// Maps the public deployment build identity endpoint.
/// </summary>
public static class SmartCitiesBuildMetadataEndpointExtensions
{
  /// <summary>
  /// Maps the public SmartCities API build metadata endpoint.
  /// </summary>
  /// <param name="app">Built SmartCities API application.</param>
  /// <returns>The mapped route handler.</returns>
  public static RouteHandlerBuilder MapSmartCitiesBuildMetadata(
    this IEndpointRouteBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    return app
      .MapGet(
        "/api/system/build",
        static (
          SmartCitiesBuildMetadata metadata) =>
            Results.Ok(metadata))
      .WithName("GetSmartCitiesBuildMetadata")
      .WithTags("System")
      .Produces<SmartCitiesBuildMetadata>(
        StatusCodes.Status200OK);
  }
}

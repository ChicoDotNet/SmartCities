using SmartCities.Api.Administration;
using SmartCities.Api.Hosting;
using SmartCities.Api.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSmartCitiesFromConfiguration(
  builder.Configuration);
builder.Services.AddSmartCitiesFeatureManagementFromConfiguration(
  builder.Configuration);
builder.Services.AddSmartCitiesAdministrationBootstrap(
  builder.Configuration);
builder.Services.AddSmartCitiesApiControllers();
builder.Services.AddSmartCitiesApiDiagnostics();
builder.Services.AddSmartCitiesApiObservability(
  builder.Configuration);
builder.Services.AddSmartCitiesAuthenticationProviders(
  builder.Configuration);
builder.Services.AddSmartCitiesAuthorization();

var app = builder.Build();

await app.ApplySmartCitiesDevelopmentDatabaseAsync();

app.UseSmartCitiesRequestObservability();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseSmartCitiesAuthenticationCanonicalization();
app.UseAuthorization();

app.MapControllers();
app.MapSmartCitiesApiDiagnostics();
app.MapSmartCitiesBuildMetadata();

app.Run();

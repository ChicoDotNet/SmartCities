using SmartCities.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSmartCitiesFromConfiguration(
  builder.Configuration);
builder.Services.AddSmartCitiesApiControllers();
builder.Services.AddSmartCitiesApiDiagnostics();
builder.Services.AddSmartCitiesApiObservability();

var app = builder.Build();

await app.ApplySmartCitiesDevelopmentDatabaseAsync();

app.UseSmartCitiesRequestObservability();
app.UseRequestLocalization();

app.MapControllers();
app.MapSmartCitiesApiDiagnostics();

app.Run();

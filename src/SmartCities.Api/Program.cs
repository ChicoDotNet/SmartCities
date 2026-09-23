using SmartCities.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSmartCitiesFromConfiguration(
  builder.Configuration);
builder.Services.AddSmartCitiesApiControllers();

var app = builder.Build();

await app.ApplySmartCitiesDevelopmentDatabaseAsync();

app.UseRequestLocalization();
app.MapControllers();

app.Run();

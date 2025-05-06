using HybridCacheExample.Configurations;
using HybridCacheExample.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<WeatherAPISettings>(builder.Configuration.GetSection("Weather"));
builder.Services.AddHttpClient("weather",(serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<WeatherAPISettings>>().Value;

    client.BaseAddress = new Uri(settings.BASE_URL ?? throw new InvalidOperationException($"Invalid BASE_URL in configuration: {settings.BASE_URL}"));
});

// simple memory cache
builder.Services.AddMemoryCache();

// distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    string? redisConnection = builder.Configuration.GetConnectionString("redis");

    options.Configuration = redisConnection ?? throw new InvalidOperationException($"Invalid BASE_URL in configuration: {redisConnection}");

    options.InstanceName = "redis";
});

builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

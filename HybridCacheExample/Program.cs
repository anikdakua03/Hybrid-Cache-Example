using HybridCacheExample.Configurations;
using HybridCacheExample.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

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
//builder.Services.AddMemoryCache();

// distributed cache
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    string? redisConnection = builder.Configuration.GetConnectionString("Redis");

//    options.Configuration = redisConnection ?? throw new InvalidOperationException($"Invalid BASE_URL in configuration: {redisConnection}");

//    options.InstanceName = "redis";
//});

// hybrid cache
//builder.Services.AddHybridCache(options =>
//{
//    options.DefaultEntryOptions = new HybridCacheEntryOptions()
//    {
//        LocalCacheExpiration = TimeSpan.FromMinutes(1), // Level 1 Cache which is in-memory, intended to be shorter than distributive cache
//        Expiration = TimeSpan.FromMinutes(5) // Level 2 Cache which is distributive memory, intended to be little longer than in-memory cache
//    };

//    // to use Redis as Level 2 distributive cache
//    // uncomment the distributive cache service
//});

// hybrid cache using Fusion Cache
string? redisConnection = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(options =>
    {
        options.Duration = TimeSpan.FromMinutes(4);
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDistributedCache(
    new RedisCache(
        new RedisCacheOptions() 
        {
            Configuration = redisConnection ?? throw new InvalidOperationException($"Invalid BASE_URL in configuration: {redisConnection}") 
        }))
    .AsHybridCache(); // this AsHybridCache() will use Microsoft's Hybrid cache's abstract class and implement under the hood

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

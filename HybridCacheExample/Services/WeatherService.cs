using HybridCacheExample.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HybridCacheExample.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WeatherAPISettings _settings;
    //private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;

    public WeatherService(IHttpClientFactory httpClientFactory, IOptions<WeatherAPISettings> settings, /*IMemoryCache memoryCache,*/ IDistributedCache distributedCache)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        //_memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string cityName, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"weather-{cityName}";

        //var cachedWeather = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        //{
        //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

        //    return await GetWeatherByCityFromClient(cityName, cancellationToken);
        //});

        string? cachedWeatherString = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(cachedWeatherString) == false)
        {
            // already cached , so deserialized to proper type
            WeatherResponse? cachedWeather = JsonConvert.DeserializeObject<WeatherResponse>(cachedWeatherString);

            return cachedWeather;
        }

        // other wise call the external api and then add to cache
        WeatherResponse? weatherResponse = await GetWeatherByCityFromClient(cityName, cancellationToken);

        if(weatherResponse is null)
        {
            // no need to cache, just return
            return weatherResponse;
        }

        await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(weatherResponse), cancellationToken);

        return weatherResponse;
    }

    private async Task<WeatherResponse?> GetWeatherByCityFromClient(string cityName, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("weather");

        string encodedCity = Uri.EscapeDataString(cityName);

        string requestURI = $"?q={encodedCity}&appid={_settings.API_KEY}";

        var response = await client.GetAsync(requestURI, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();

            return weather;
        }
        else
        {
            return null;
        }
    }
}

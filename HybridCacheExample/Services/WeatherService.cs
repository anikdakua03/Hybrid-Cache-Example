using HybridCacheExample.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HybridCacheExample.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WeatherAPISettings _settings;
    private readonly IMemoryCache _memoryCache;

    public WeatherService(IHttpClientFactory httpClientFactory, IOptions<WeatherAPISettings> settings, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _memoryCache = memoryCache;
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string cityName, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"weather-{cityName}";

        var cachedWeather = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            return await GetWeatherByCityFromClient(cityName, cancellationToken);
        });

        return cachedWeather;
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

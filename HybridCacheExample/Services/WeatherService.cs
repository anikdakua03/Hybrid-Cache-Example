using HybridCacheExample.Configurations;
using Microsoft.Extensions.Options;

namespace HybridCacheExample.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WeatherAPISettings _settings;

    public WeatherService(IHttpClientFactory httpClientFactory, IOptions<WeatherAPISettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public async Task<WeatherResponse?> GetWeatherByCityAsync(string cityName)
    {
        return await GetWeatherByCityFromClient(cityName);
    }

    private async Task<WeatherResponse?> GetWeatherByCityFromClient(string cityName)
    {
        var client = _httpClientFactory.CreateClient("weather");

        string encodedCity = Uri.EscapeDataString(cityName);

        string requestURI = $"?q={encodedCity}&appid={_settings.API_KEY}";

        var response = await client.GetAsync(requestURI);

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

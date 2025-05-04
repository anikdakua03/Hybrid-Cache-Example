namespace HybridCacheExample.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherByCityAsync(string cityName, CancellationToken cancellationToken = default);
}
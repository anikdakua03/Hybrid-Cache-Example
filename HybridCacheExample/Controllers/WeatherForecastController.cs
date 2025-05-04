using HybridCacheExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace HybridCacheExample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherService _weatherService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherService weatherService)
    {
        _logger = logger;
        _weatherService = weatherService;
    }

    [HttpGet("GetWeatherByCityName/{cityName}")]
    public async Task<IActionResult> GetWeatherByCityName(string cityName, CancellationToken cancellationToken = default)
    {
        var weather = await _weatherService.GetWeatherByCityAsync(cityName, cancellationToken);

        if (weather == null)
        {
            return NotFound("No weather data found");
        }

        return Ok(weather);
    }
}

namespace SmartHeater.Services;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;

    public OpenWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public double ReadTemperatureC()
    {
        throw new NotImplementedException();
    }
}

namespace SmartHeater.BL;

public class OpenWeatherService
{
    private readonly HttpClient _httpClient;

    public OpenWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public double ReadTemperature()
    {
        throw new NotImplementedException();
    }
}

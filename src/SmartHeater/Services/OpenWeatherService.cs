namespace SmartHeater.Services;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IpApiService _ipApiService;
    private readonly string _apiKey;

    public OpenWeatherService(HttpClient httpClient, IConfiguration configuration, IpApiService ipApiService)
    {
        _httpClient = httpClient;
        _ipApiService = ipApiService;
        _apiKey = configuration["OpenWeather:ApiKey"];
    }

    public async Task<double> ReadTemperatureC()
    {
        (var lat, var lon) = await _ipApiService.GetLatitudeLongitude();

        var requestUri = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
        var weatherModel = await _httpClient.GetFromJsonAsync<OpenWeatherModel>(requestUri);

        return Convert.ToDouble(weatherModel?.Data?["temp"].ToString(), CultureInfo.InvariantCulture);
    }

    private class OpenWeatherModel
    {
        [JsonPropertyName("main")]
        public Dictionary<string, object>? Data { get; set; }
    }
}

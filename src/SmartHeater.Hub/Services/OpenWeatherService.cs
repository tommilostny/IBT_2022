namespace SmartHeater.Hub.Services;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ICoordinatesService _coordsService;
    private readonly string _apiKey;

    public OpenWeatherService(HttpClient httpClient, IConfiguration configuration, ICoordinatesService ipApiService)
    {
        _httpClient = httpClient;
        _coordsService = ipApiService;
        _apiKey = configuration["OpenWeather:ApiKey"];
    }

    public async Task<double?> ReadTemperatureC()
    {
        (var lat, var lon) = await _coordsService.GetLatitudeLongitude();
        if (lat is null || lon is null)
        {
            return null;
        }
        var requestUri = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
        try
        {
            var weatherModel = await _httpClient.GetFromJsonAsync<OpenWeatherModel>(requestUri);
            return Convert.ToDouble(weatherModel?.Data?["temp"].ToString(), CultureInfo.InvariantCulture);
        }
        catch
        {
            Console.Error.WriteLine("Error while getting weather.");
            return null;
        }
    }

    private class OpenWeatherModel
    {
        [JsonProperty("main")]
        public Dictionary<string, object>? Data { get; set; }
    }
}

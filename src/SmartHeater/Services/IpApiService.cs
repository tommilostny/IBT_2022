namespace SmartHeater.Services;

public class IpApiService
{
    private readonly HttpClient _httpClient;

    //Cache public IP addres and coordinatest (Hub won't be moving).
    private string? _ipAddress;
    private double[]? _latLon;

    public IpApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(double, double)> GetLatitudeLongitude()
    {
        if (string.IsNullOrWhiteSpace(_ipAddress))
        {
            _ipAddress = await GetPublicIpAddress();
            _latLon = null;
        }
        if (_latLon is null || _latLon.Length != 2)
        {
            var latLonStrings = await GetLatLonStrings(_ipAddress);

            _latLon = new double[2];
            for (int i = 0; i < 2; i++)
            {
                _latLon[i] = Convert.ToDouble(latLonStrings[i], CultureInfo.InvariantCulture);
            }
        }
        return (_latLon[0], _latLon[1]);
    }

    private async Task<string> GetPublicIpAddress()
    {
        var response = await _httpClient.GetAsync("http://icanhazip.com");
        return (await response.Content.ReadAsStringAsync()).Trim();
    }

    private async Task<string[]> GetLatLonStrings(string ipAddress)
    {
        var response = await _httpClient.GetAsync($"https://ipapi.co/{ipAddress}/latlong");
        return (await response.Content.ReadAsStringAsync()).Trim().Split(',');
    }
}

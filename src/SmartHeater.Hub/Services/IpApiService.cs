namespace SmartHeater.Hub.Services;

public class IpApiService : ICoordinatesService
{
    private readonly HttpClient _httpClient;

    //Cache public IP addres and coordinates to reduce traffic (Hub won't be moving).
    private string? _ipAddress;
    private double[]? _latLon;

    public IpApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(double?, double?)> GetLatitudeLongitude()
    {
        if (string.IsNullOrWhiteSpace(_ipAddress))
        {
            _latLon = null;
            if ((_ipAddress = await GetPublicIpAddress()) is null)
            {
                return (null, null);
            }
        }
        if (_latLon is null || _latLon.Length != 2)
        {
            var latLonStrings = await GetCoordStrings(_ipAddress);
            if (latLonStrings is null || latLonStrings.Length != 2)
            {
                return (null, null);
            }
            _latLon = new double[2];
            for (int i = 0; i < 2; i++)
            {
                _latLon[i] = Convert.ToDouble(latLonStrings[i], CultureInfo.InvariantCulture);
            }
        }
        return (_latLon[0], _latLon[1]);
    }

    private async Task<string?> GetPublicIpAddress()
    {
        try
        {
            var response = await _httpClient.GetAsync("http://icanhazip.com");
            return (await response.Content.ReadAsStringAsync()).Trim();
        }
        catch
        {
            Console.Error.WriteLine("Error while getting public IP.");
            return null;
        }
    }

    private async Task<string[]?> GetCoordStrings(string ipAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://ipapi.co/{ipAddress}/latlong");
            return (await response.Content.ReadAsStringAsync()).Trim().Split(',');
        }
        catch
        {
            Console.Error.WriteLine("Error while getting latitude and longitude.");
            return null;
        }
    }
}

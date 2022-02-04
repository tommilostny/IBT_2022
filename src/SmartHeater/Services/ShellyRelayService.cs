using System.Globalization;
using System.Text.Json.Serialization;

namespace SmartHeater.Services;

public class ShellyRelayService : IHeaterService
{
    private readonly HttpClient _httpClient;
    private readonly ShellyRelayStatus _statusCache = new();


    public ShellyRelayService(HttpClient httpClient, string ipAddress)
    {
        _httpClient = httpClient;
        IPAddress = ipAddress;
    }

    public string IPAddress { get; }

    private string Relay0Url => $"http://{IPAddress}/relay/0";

    private string StatusUrl => $"http://{IPAddress}/status";

    public async Task<bool?> GetStatus(bool fromCache = false)
    {
        if (fromCache)
        {
            return _statusCache.IsTurnedOn;
        }
        var response = await _httpClient.GetFromJsonAsync<ShellyRelayStatus>(Relay0Url);
        return _statusCache.IsTurnedOn = response?.IsTurnedOn;
    }

    public async Task<double?> ReadTemperature(bool fromCache = false)
    {
        if (fromCache)
        {
            return _statusCache.Temperature;
        }
        var response = await _httpClient.GetFromJsonAsync<ShellyTemperatureStatus>(StatusUrl);
        try
        {   
            _statusCache.Temperature = Convert.ToDouble(response?.ExtTemperature?["0"]["tC"].ToString(), CultureInfo.InvariantCulture);
            return _statusCache.Temperature;
        }
        catch
        {
            return null;
        }
    }

    public async Task<double?> ReadPower(bool fromCache = false)
    {
        if (fromCache)
        {
            return _statusCache.Power;
        }
        var response = await _httpClient.GetFromJsonAsync<ShellyPowerStatus>(StatusUrl);
        try
        {
            _statusCache.Power = Convert.ToDouble(response?.Meters?.First()["power"].ToString(), CultureInfo.InvariantCulture);
            return _statusCache.Power;
        }
        catch
        {
            return null;
        }
    }

    public async Task TurnOn() => await SendTurnRequest("on");

    public async Task TurnOff() => await SendTurnRequest("off");

    private async Task SendTurnRequest(string state)
    {
        var data = new[]
        {
            new KeyValuePair<string, string>("turn", state)
        };
        try
        {
            await _httpClient.PostAsync(Relay0Url, new FormUrlEncodedContent(data));
        }
        catch (Exception ex)
        {
            //TODO: logger?
            Console.WriteLine(ex.Message);
        }
    }

    private class ShellyRelayStatus
    {
        [JsonPropertyName("ison")]
        public bool? IsTurnedOn { get; set; }

        [JsonIgnore]
        public double? Temperature { get; set; }

        [JsonIgnore]
        public double? Power { get; set; }
    };

    private class ShellyTemperatureStatus
    {
        [JsonPropertyName("ext_temperature")]
        public Dictionary<string, Dictionary<string, object>>? ExtTemperature { get; set; }
    };

    private class ShellyPowerStatus
    {
        [JsonPropertyName("meters")]
        public List<Dictionary<string, object>>? Meters { get; set; }
    };
}

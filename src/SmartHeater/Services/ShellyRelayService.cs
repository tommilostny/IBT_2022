using System.Globalization;
using System.Text.Json.Serialization;

namespace SmartHeater.Services;

public class ShellyRelayService : IHeaterService
{
    private readonly HttpClient _httpClient;  
    

    public ShellyRelayService(HttpClient httpClient, string ipAddress)
    {
        _httpClient = httpClient;
        IPAddress = ipAddress;
    }

    public string IPAddress { get; }

    private string Relay0Url => $"http://{IPAddress}/relay/0";

    private string StatusUrl => $"http://{IPAddress}/status";

    public async Task<bool?> GetStatus()
    {
        var response = await _httpClient.GetFromJsonAsync<ShellyRelayStatus>(Relay0Url);
        return response?.IsTurnedOn;
    }

    public async Task<double> ReadTemperature()
    {
        var response = await _httpClient.GetFromJsonAsync<ShellyTemperatureStatus>(StatusUrl);
        try
        {
            return Convert.ToDouble(response?.Temperature?["0"]["tC"].ToString(), CultureInfo.InvariantCulture);
        }
        catch
        {
            return double.NaN;
        }
    }

    public async Task<double> ReadPower()
    {
        var response = await _httpClient.GetFromJsonAsync<ShellyPowerStatus>(StatusUrl);
        try
        {
            return Convert.ToDouble(response?.Meters?.First()["power"].ToString(), CultureInfo.InvariantCulture);
        }
        catch
        {
            return double.NaN;
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
        public bool IsTurnedOn { get; set; }
    };

    private class ShellyTemperatureStatus
    {
        [JsonPropertyName("ext_temperature")]
        public Dictionary<string, Dictionary<string, object>>? Temperature { get; set; }
    };

    private class ShellyPowerStatus
    {
        [JsonPropertyName("meters")]
        public List<Dictionary<string, object>>? Meters { get; set; }
    };
}

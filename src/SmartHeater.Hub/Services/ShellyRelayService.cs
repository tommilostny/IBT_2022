using System.Text.Json.Serialization;

namespace SmartHeater.Hub.Services;

public class ShellyRelayService : IHeaterControlService
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

    public async Task<HeaterStatusModel?> GetStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ShellyRelayStatus>(StatusUrl);
            var status = new HeaterStatusModel(IPAddress, DateTime.UtcNow)
            {
                IsTurnedOn = ReadRelayState(response),
                Temperature = ReadTemperature(response),
                Power = ReadPower(response)
            };
            return status;
        }
        catch
        {
            return null;
        }
    }

    public async Task TurnOnAsync() => await SendTurnRequestAsync("on");

    public async Task TurnOffAsync() => await SendTurnRequestAsync("off");

    private async Task SendTurnRequestAsync(string state)
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
            Console.Error.WriteLine($"{DateTime.Now}: {ex.Message}");
        }
    }

    private static bool? ReadRelayState(ShellyRelayStatus? relayStatus)
    {
        try
        {
            return Convert.ToBoolean(relayStatus?.Relays?.First()["ison"].ToString());
        }
        catch
        {
            return null;
        }
    }

    private static double? ReadTemperature(ShellyRelayStatus? relayStatus)
    {
        try
        {
            return Convert.ToDouble(relayStatus?.ExtTemperature?.First().Value["tC"].ToString(), CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static double? ReadPower(ShellyRelayStatus? relayStatus)
    {
        try
        {
            return Convert.ToDouble(relayStatus?.Meters?.First()["power"].ToString(), CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private class ShellyRelayStatus
    {
        [JsonPropertyName("relays")]
        public List<Dictionary<string, object>>? Relays { get; set; }

        [JsonPropertyName("ext_temperature")]
        public Dictionary<string, Dictionary<string, object>>? ExtTemperature { get; set; }

        [JsonPropertyName("meters")]
        public List<Dictionary<string, object>>? Meters { get; set; }
    };
}

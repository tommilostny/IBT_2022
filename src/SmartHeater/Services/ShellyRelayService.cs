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

    public async Task<bool?> GetStatus()
    {
        var response = await _httpClient.GetFromJsonAsync<ShellyStatus>(Relay0Url);
        return response?.IsTurnedOn;
    }

    public async Task<double> ReadTemperature()
    {
        throw new NotImplementedException();
    }

    public async Task TurnOff() => await SendTurnRequest("on");

    public async Task TurnOn() => await SendTurnRequest("off");

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

    private class ShellyStatus
    {
        [JsonPropertyName("ison")]
        public bool IsTurnedOn { get; set; }
    };
}

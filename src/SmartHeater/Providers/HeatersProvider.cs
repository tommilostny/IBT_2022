using SmartHeater.Services;

namespace SmartHeater.Providers;

public class HeatersProvider
{
    private readonly HttpClient _httpClient;
    private const string _heatersJsonFile = "heaters.json";

    public HeatersProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ICollection<IHeaterService>> GetHeaters()
    {
        var heaters = new List<IHeaterService>();
        foreach (var ipAddress in await ReadHeatersFromJson())
        {
            heaters.Add(new ShellyRelayService(_httpClient, ipAddress));
        }
        return heaters;
    }

    public async Task Register(string ipAddress)
    {
        var ipAddresses = await ReadHeatersFromJson();
        ipAddresses.Add(ipAddress);
        await WriteHeatersToJson(ipAddresses);
    }

    public async Task Remove(string ipAddress)
    {
        var ipAddresses = await ReadHeatersFromJson();
        ipAddresses.Remove(ipAddress);
        await WriteHeatersToJson(ipAddresses);
    }

    private static async Task<ICollection<string>> ReadHeatersFromJson()
    {
        var jsonStr = File.Exists(_heatersJsonFile) ? await File.ReadAllTextAsync(_heatersJsonFile) : null;
        return JsonSerializer.Deserialize<List<string>>(jsonStr ?? "[]") ?? new();
    }

    private static async Task WriteHeatersToJson(ICollection<string> heaters)
    {
        var jsonStr = JsonSerializer.Serialize(heaters);
        await File.WriteAllTextAsync(_heatersJsonFile, jsonStr);
    }
}

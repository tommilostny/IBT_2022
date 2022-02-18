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

    public async Task<ICollection<IHeaterService>> GetHeaterServices()
    {
        var heaters = new List<IHeaterService>();
        foreach (var heater in await ReadHeaters())
        {
            var heaterService = heater.HeaterType switch
            {
                HeaterTypes.Shelly1PM => new ShellyRelayService(_httpClient, heater.IpAddress),
                _ => throw new InvalidOperationException()
            };
            heaters.Add(heaterService);
        }
        return heaters;
    }

    public async Task InsertUpdate(HeaterListModel heater)
    {
        var heaters = await ReadHeaters();
        var existingHeater = heaters.FirstOrDefault(h => h.IpAddress == heater.IpAddress);

        if (existingHeater is not null)
        {
            heaters.Remove(existingHeater);
        }
        heaters.Add(heater);
        await WriteHeaters(heaters);
    }

    public async Task Delete(string ipAddress)
    {
        var heaters = await ReadHeaters();
        var heaterToRemove = heaters.FirstOrDefault(h => h.IpAddress == ipAddress);

        if (heaterToRemove is not null)
        {
            heaters.Remove(heaterToRemove);
            await WriteHeaters(heaters);
        }
    }

    public async Task<ICollection<HeaterListModel>> ReadHeaters()
    {
        var jsonStr = File.Exists(_heatersJsonFile) ? await File.ReadAllTextAsync(_heatersJsonFile) : null;
        return JsonSerializer.Deserialize<List<HeaterListModel>>(jsonStr ?? "[]") ?? new();
    }

    private static async Task WriteHeaters(ICollection<HeaterListModel> heaters)
    {
        var jsonStr = JsonSerializer.Serialize(heaters);
        await File.WriteAllTextAsync(_heatersJsonFile, jsonStr);
    }
}

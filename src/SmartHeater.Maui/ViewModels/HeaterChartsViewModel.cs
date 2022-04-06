namespace SmartHeater.Maui.ViewModels;

public class HeaterChartsViewModel : BindableObject
{
    private readonly SettingsProvider _settingsProvider;
    private readonly HttpClient _httpClient;

    public HeaterChartsViewModel(SettingsProvider settingsProvider, HttpClient httpClient)
    {
        _settingsProvider = settingsProvider;
        _httpClient = httpClient;
    }

    public ObservableCollection<DbRecordModel> PowerData { get; } = new();

    public ObservableCollection<DbRecordModel> TemperatureData { get; } = new();

    public async Task Load(string ipAddress)
    {
        PowerData.Clear();
        TemperatureData.Clear();

        var uri = $"{_settingsProvider.HubUri}/heaters/{ipAddress}/history/3h/power";
        foreach (var item in await _httpClient.GetFromJsonAsync<List<DbRecordModel>>(uri))
        {
            item.MeasurementTime = item.MeasurementTime.Value.ToLocalTime();
            PowerData.Add(item);
        }

        uri = $"{_settingsProvider.HubUri}/heaters/{ipAddress}/history/3h/temperature";
        foreach (var item in await _httpClient.GetFromJsonAsync<List<DbRecordModel>>(uri))
        {
            item.MeasurementTime = item.MeasurementTime.Value.ToLocalTime();
            TemperatureData.Add(item);
        }
    }
}

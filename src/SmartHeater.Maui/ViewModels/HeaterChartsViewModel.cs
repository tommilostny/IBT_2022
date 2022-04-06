namespace SmartHeater.Maui.ViewModels;

public class HeaterChartsViewModel : BindableObject
{
    private readonly SettingsProvider _settingsProvider;
    private readonly HttpClient _httpClient;

    public HeaterChartsViewModel(SettingsProvider settingsProvider, HttpClient httpClient)
    {
        _settingsProvider = settingsProvider;
        _httpClient = httpClient;
        PeriodSelectorViewModel = new(Load);
    }

    public PeriodSelectorViewModel PeriodSelectorViewModel { get; }

    public string IpAddress { get; set; } = null;

    public ObservableCollection<DbRecordModel> PowerData { get; } = new();

    public ObservableCollection<DbRecordModel> TemperatureData { get; } = new();

    private bool _loaded = false;
    public bool Loaded
    {
        get => _loaded;
        set
        {
            _loaded = value;
            OnPropertyChanged(nameof(Loaded));
        }
    }

    private bool _loading = false;
    public bool IsLoading
    {
        get => _loading;
        set
        {
            _loading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    private async void Load()
    {
        if (string.IsNullOrWhiteSpace(IpAddress))
            return;

        IsLoading = true;
        Loaded = false;
        PowerData.Clear();
        TemperatureData.Clear();

        var uri = $"{_settingsProvider.HubUri}/heaters/{IpAddress}/history/{PeriodSelectorViewModel.SelectedPeriod}/power";
        foreach (var item in await _httpClient.GetFromJsonAsync<DbRecordModel[]>(uri))
        {
            item.MeasurementTime = item.MeasurementTime.Value.ToLocalTime();
            PowerData.Add(item);
        }

        uri = $"{_settingsProvider.HubUri}/heaters/{IpAddress}/history/{PeriodSelectorViewModel.SelectedPeriod}/temperature";
        foreach (var item in await _httpClient.GetFromJsonAsync<DbRecordModel[]>(uri))
        {
            item.MeasurementTime = item.MeasurementTime.Value.ToLocalTime();
            TemperatureData.Add(item);
        }

        Loaded = true;
        IsLoading = false;
    }
}

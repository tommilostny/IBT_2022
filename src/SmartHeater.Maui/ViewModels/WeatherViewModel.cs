namespace SmartHeater.Maui.ViewModels;

public class WeatherViewModel : BindableObject
{
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;

    public WeatherViewModel(HttpClient httpClient, SettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        PeriodSelectorViewModel = new(() => Load(false, true));
        Load(true, false);
    }

    public PeriodSelectorViewModel PeriodSelectorViewModel { get; }

    private ICommand _reloadCommand;
    public ICommand ReloadCommand => _reloadCommand ??= new Command(() => Load(true, true));

    public ObservableCollection<DbRecordModel> Data { get; } = new();

    private bool _loadError = false;
    public bool LoadError
    {
        get => _loadError;
        set
        {
            _loadError = value;
            OnPropertyChanged(nameof(LoadError));
        }
    }

    private double _temperature;
    public double TemperatureC
    {
        get => _temperature;
        set
        {
            _temperature = value;
            OnPropertyChanged(nameof(TemperatureC));
        }
    }

    private bool _temperatureIsValid = false;
    public bool TemperatureIsValid
    {
        get => _temperatureIsValid;
        set
        {
            _temperatureIsValid = value;
            OnPropertyChanged(nameof(TemperatureIsValid));
        }
    }

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    private bool _historyLoaded = false;
    public bool HistoryLoaded
    {
        get => _historyLoaded;
        set
        {
            _historyLoaded = value;
            OnPropertyChanged(nameof(HistoryLoaded));
        }
    }

    private async void Load(bool loadCurrent, bool loadHistory)
    {
        IsLoading = true;
        LoadError = false;
        try
        {
            if (loadCurrent)
            {
                await LoadCurrentWeatherAsync();
            }
            if (loadHistory && (!loadCurrent || HistoryLoaded))
            {
                await LoadWeatherHistoryAsync();
            }
        }
        catch
        {
            LoadError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadWeatherHistoryAsync()
    {
        HistoryLoaded = false;

        var uri = $"{_settingsProvider.HubUri}/heaters/192.168.1.253/history/{PeriodSelectorViewModel.SelectedPeriod}/weather";
        Data.Clear();
        foreach (var item in await _httpClient.GetFromJsonAsync<DbRecordModel[]>(uri))
        {
            item.MeasurementTime = item.MeasurementTime.Value.ToLocalTime();
            Data.Add(item);
        }

        HistoryLoaded = true;
    }

    private async Task LoadCurrentWeatherAsync()
    {
        TemperatureIsValid = false;

        var uri = $"{_settingsProvider.HubUri}/weather";
        TemperatureC = await _httpClient.GetFromJsonAsync<double>(uri);
        
        TemperatureIsValid = true;
    }
}

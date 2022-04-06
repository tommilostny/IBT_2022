namespace SmartHeater.Maui.ViewModels;

public class HeatersViewModel : BindableObject
{
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;

    public HeatersViewModel(HttpClient httpClient, SettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        Load();
    }

    private ICommand _addCommand;
    public ICommand AddCommand => _addCommand ??= new Command(AddButtonClick);

    private ICommand _loadCommand;
    public ICommand LoadCommand => _loadCommand ??= new Command(Load);

    private ICommand _selectionCommand;
    public ICommand SelectionCommand => _selectionCommand ??= new Command(ShowDetail);

    public ObservableCollection<HeaterListModel> Heaters { get; } = new();

    private HeaterListModel _model;
    public HeaterListModel SelectedHeater
    {
        get => _model;
        set
        {
            _model = value;
            OnPropertyChanged(nameof(SelectedHeater));
        }
    }

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

    private bool _heatersEmpty = false;
    public bool HeatersCollectionEmpty
    {
        get => _heatersEmpty;
        set
        {
            _heatersEmpty = value;
            OnPropertyChanged(nameof(HeatersCollectionEmpty));
        }
    }

    public string LoadErrorMessage => string.IsNullOrWhiteSpace(_settingsProvider.HubIpAddress)
        ? "To load heaters, please, set up the Hub IP address in settings."
        : "Unable to load heater data.";

    public async Task UpdateHeatersFromHttpAsync(HttpResponseMessage response)
    {
        var jsonStr = await response.Content.ReadAsStringAsync();
        var updatedHeaters = JsonConvert.DeserializeObject<List<HeaterListModel>>(jsonStr);

        Heaters.Clear();
        foreach (var heater in updatedHeaters)
        {
            Heaters.Add(heater);
        }
        HeatersCollectionEmpty = Heaters.Count == 0;
    }

    private async void AddButtonClick()
    {
        await Shell.Current.GoToAsync(nameof(AddHeaterPage));
    }

    private async void Load()
    {
        HeatersCollectionEmpty = false;
        IsLoading = true;
        LoadError = false;
        Heaters.Clear();
        try
        {
            var uri = $"{_settingsProvider.HubUri}/heaters";
            var heaters = await _httpClient.GetFromJsonAsync<HeaterListModel[]>(uri);
            foreach (var heater in heaters)
                Heaters.Add(heater);
        }
        catch
        {
            LoadError = true;
        }
        finally
        {
            IsLoading = false;
            HeatersCollectionEmpty = !LoadError && Heaters.Count == 0;
        }
    }

    private async void ShowDetail()
    {
        if (SelectedHeater is not null)
        {
            var uri = $"HeaterDetailPage?ipAddress={SelectedHeater.IpAddress}";
            await Shell.Current.GoToAsync(uri);
            SelectedHeater = null;
        }
    }
}

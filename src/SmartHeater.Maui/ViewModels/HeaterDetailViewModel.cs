using System.Web;

namespace SmartHeater.Maui.ViewModels;

public class HeaterDetailViewModel : BindableObject, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;
    private readonly HeatersViewModel _heatersViewModel;
    
    private ICommand _deleteCommand;
    public ICommand DeleteCommand => _deleteCommand ??= new Command(Delete);

    private ICommand _reloadCommand;
    public ICommand ReloadCommand => _reloadCommand ??= new Command(ReLoad);

    private ICommand _editCommand;
    public ICommand EditCommand => _editCommand ??= new Command(GoToEditPage);

    public HeaterDetailViewModel(HttpClient httpClient, SettingsProvider settingsProvider, HeatersViewModel heatersViewModel)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        _heatersViewModel = heatersViewModel;
        HeaterChartsViewModel = new(settingsProvider, httpClient);
    }

    public HeaterChartsViewModel HeaterChartsViewModel { get; set; }

    private HeaterDetailModel _heaterDetailModel;
    public HeaterDetailModel HeaterDetail
    {
        get => _heaterDetailModel;
        set
        {
            HeaterChartsViewModel.IpAddress = value.IpAddress;
            _heaterDetailModel = value;
            OnPropertyChanged(nameof(HeaterDetail));
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

    private bool _isLoaded = false;
    public bool IsLoaded
    {
        get => _isLoaded;
        set
        {
            _isLoaded = value;
            OnPropertyChanged(nameof(IsLoaded));
        }
    }

    private bool _loadingError = false;
    public bool LoadingError
    {
        get => _loadingError;
        set
        {
            _loadingError = value;
            OnPropertyChanged(nameof(LoadingError));
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var ipAddress = HttpUtility.UrlDecode(query["ipAddress"].ToString());
        await GetHeaterDataAsync(ipAddress);
        if (LoadingError)
        {
            _heatersViewModel.LoadError = true;
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void ReLoad()
    {
        await GetHeaterDataAsync(HeaterDetail.IpAddress);
    }

    private async Task GetHeaterDataAsync(string ipAddress)
    {
        IsLoading = true;
        IsLoaded = false;
        LoadingError = false;
        try
        {
            var uri = $"{_settingsProvider.HubUri}/heaters/{ipAddress}";
            HeaterDetail = await _httpClient.GetFromJsonAsync<HeaterDetailModel>(uri);
            IsLoaded = true;
        }
        catch
        {
            LoadingError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void Delete()
    {
        var delete = await App.Current.MainPage.DisplayAlert("Delete", $"Delete heater '{HeaterDetail.IpAddress}'?", "Yes", "No");
        if (delete)
        {
            var uri = $"{_settingsProvider.HubUri}/heaters/{HeaterDetail.IpAddress}";
            var response = await _httpClient.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                await _heatersViewModel.UpdateHeatersFromHttpAsync(response);
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    private async void GoToEditPage()
    {
        var uri = $"{nameof(AddHeaterPage)}?IpAddress={HeaterDetail.IpAddress}";
        uri += $"&Name={HeaterDetail.Name}";
        uri += $"&HeaterType={(int)HeaterDetail.HeaterType}";
        uri += $"&ReferenceTemperature={HeaterDetail.ReferenceTemperature}";
        await Shell.Current.GoToAsync(uri);
    }
}

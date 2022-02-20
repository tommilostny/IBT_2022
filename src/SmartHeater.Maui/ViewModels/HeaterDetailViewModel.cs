using System.Web;

namespace SmartHeater.Maui.ViewModels;

public class HeaterDetailViewModel : BindableObject, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;
    private readonly HeatersViewModel _heatersViewModel;
    
    private ICommand _deleteCommand;
    public ICommand DeleteCommand => _deleteCommand ??= new Command(DeleteHeaterAsync);

    public HeaterDetailViewModel(HttpClient httpClient, SettingsProvider settingsProvider, HeatersViewModel heatersViewModel)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        _heatersViewModel = heatersViewModel;
    }

    private HeaterDetailModel _heaterDetailModel;
    public HeaterDetailModel HeaterDetail
    {
        get => _heaterDetailModel;
        set
        {
            _heaterDetailModel = value;
            OnPropertyChanged(nameof(HeaterDetail));
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var ipAddress = HttpUtility.UrlDecode(query["ipAddress"].ToString());
        var uri = $"{_settingsProvider.HubUri}/heaters/{ipAddress}";

        HeaterDetail = await _httpClient.GetFromJsonAsync<HeaterDetailModel>(uri);
    }

    private async void DeleteHeaterAsync()
    {
        //TODO: Some confirmation dialog.
        var uri = $"{_settingsProvider.HubUri}/heaters/{HeaterDetail.IpAddress}";
        var response = await _httpClient.DeleteAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var heaterToRemove = _heatersViewModel.Heaters.FirstOrDefault(h => h.IpAddress == HeaterDetail.IpAddress);
            _heatersViewModel.Heaters.Remove(heaterToRemove);
            await Shell.Current.GoToAsync("..");
        }
    }
}

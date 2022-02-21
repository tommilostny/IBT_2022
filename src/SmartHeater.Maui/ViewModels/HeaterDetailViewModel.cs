﻿using System.Web;

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

    public HeaterDetailViewModel(HttpClient httpClient, SettingsProvider settingsProvider, HeatersViewModel heatersViewModel)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        _heatersViewModel = heatersViewModel;
    }

    private HeaterDetailModel _heaterDetailModel = new(string.Empty, "Loading...");
    public HeaterDetailModel HeaterDetail
    {
        get => _heaterDetailModel;
        set
        {
            _heaterDetailModel = value;
            OnPropertyChanged(nameof(HeaterDetail));
            ShowLastMeasurement = value.LastMeasurement is not null;
        }
    }

    private bool _showLastMeasurement = false;
    public bool ShowLastMeasurement
    {
        get => _showLastMeasurement;
        set
        {
            _showLastMeasurement = value;
            OnPropertyChanged(nameof(ShowLastMeasurement));
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
        await GetHeaterInfo(ipAddress);
        if (LoadingError)
        {
            _heatersViewModel.LoadError = true;
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void ReLoad()
    {
        await GetHeaterInfo(HeaterDetail.IpAddress);
    }

    private async Task GetHeaterInfo(string ipAddress)
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
        //TODO: Some confirmation dialog.
        var uri = $"{_settingsProvider.HubUri}/heaters/{HeaterDetail.IpAddress}";
        var response = await _httpClient.DeleteAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var heaterToRemove = _heatersViewModel.Heaters.FirstOrDefault(h => h.IpAddress == HeaterDetail.IpAddress);
            _heatersViewModel.DeleteHeaterFromCollectionView(heaterToRemove);
            await Shell.Current.GoToAsync("..");
        }
    }
}

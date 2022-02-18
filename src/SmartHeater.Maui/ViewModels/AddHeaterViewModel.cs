﻿namespace SmartHeater.Maui.ViewModels;

public class AddHeaterViewModel : BindableObject
{
    private readonly HttpClient _httpClient;
    private readonly HeatersViewModel _heatersViewModel;

    public List<HeaterTypes> HeaterTypesList { get; } = new();

    public AddHeaterViewModel(HttpClient httpClient, HeatersViewModel heatersViewModel)
    {
        _httpClient = httpClient;
        _heatersViewModel = heatersViewModel;
        foreach (var item in Enum.GetValues(typeof(HeaterTypes)))
        {
            HeaterTypesList.Add((HeaterTypes)item);
        }
    }

    private ICommand _addHeaterCommand;
    public ICommand AddHeaterCommand => _addHeaterCommand ??= new Command(AddHeater);

    private string _ipAddress = string.Empty;
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            ShowError = false;
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            ShowError = false;
        }
    }

    private HeaterTypes _type = HeaterTypes.Shelly1PM;
    public HeaterTypes HeaterType
    {
        get => _type;
        set
        {
            _type = value;
            ShowError = false;
        }
    } 

    private bool _showError = false;
    public bool ShowError
    {
        get => _showError;
        set
        {
            _showError = value;
            OnPropertyChanged(nameof(ShowError));
        }
    }

    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    private async void AddHeater()
    {
        try
        {
            //Check if input is a valid IP address, this throws FormatException if invalid.
            System.Net.IPAddress.Parse(_ipAddress);

            //Register heater with valid IP address.
            var heater = new HeaterListModel(_ipAddress, _name, _type);
            var response = await _httpClient.PostAsJsonAsync("heaters/insert-update", heater);
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Unable to add heater.";
                ShowError = true;
                return;
            }
            var heaterInCollView = _heatersViewModel.Heaters.FirstOrDefault(h => h.IpAddress == _ipAddress);
            if (heaterInCollView is not null)
            {
                _heatersViewModel.Heaters.Remove(heaterInCollView);
            }
            _heatersViewModel.Heaters.Add(heater);

            //HttpClient did not throw an exception, heater added successfully, go back.
            await Shell.Current.GoToAsync("..");
        }
        catch (FormatException)
        {
            ErrorMessage = $"'{_ipAddress}' is not valid IP address.";
            ShowError = true;
        }
        catch
        {
            ErrorMessage = "An error has occured while contacting the SmartHeater Hub.";
            ShowError = true;
        }
    }
}

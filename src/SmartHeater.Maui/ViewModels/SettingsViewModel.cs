using System.Net;

namespace SmartHeater.Maui.ViewModels;

public class SettingsViewModel : BindableObject
{
    private ICommand _manualConnectCommand;
    public ICommand ManualConnectCommand => _manualConnectCommand ??= new Command(ManualConnect);

    private readonly SettingsProvider _settingsProvider;
    private readonly HttpClient _httpClient;

    public SettingsViewModel(SettingsProvider settingsProvider, HttpClient httpClient)
    {
        _settingsProvider = settingsProvider;
        _httpClient = httpClient;
        _hubIpAddress = _settingsProvider.HubIpAddress;
    }

    private string _hubIpAddress;
    public string HubIpAddress
    {
        get => _hubIpAddress;
        set
        {
            _hubIpAddress = value;
            ShowError = false;
            OnPropertyChanged(nameof(HubIpAddress));
        }
    }

    private bool? _isConnected = null;
    public bool? IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
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

    public async Task CheckAvailabilityAsync()
    {
        try
        {
            var reply = await _httpClient.GetAsync($"http://{HubIpAddress}/availability-test");

            if (!reply.IsSuccessStatusCode)
            {
                throw new Exception($"Could not connect to {HubIpAddress}.");
            }
            _settingsProvider.SetHubAddress(HubIpAddress);
            IsConnected = true;
            ShowError = false;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
            ShowError = true;
            IsConnected = false;
        }
    }

    private async void ManualConnect()
    {
        //Start connecting.
        IsConnected = null;

        if (HubIpAddress == "localhost")
        {
            _settingsProvider.SetHubAddress(HubIpAddress);
            IsConnected = true;
            return;
        }

        //Parse given IP address.
        IPAddress iPAddress;
        try
        {
            iPAddress = IPAddress.Parse(HubIpAddress);
        }
        catch (FormatException)
        {
            ErrorMessage = $"'{HubIpAddress}' is not valid IP address.";
            ShowError = true;
            IsConnected = false;
            return;
        }

        //Check connection using ICMP ping.
        await CheckAvailabilityAsync();
    }
}

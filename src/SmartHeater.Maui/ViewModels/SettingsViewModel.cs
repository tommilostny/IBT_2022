namespace SmartHeater.Maui.ViewModels;

public class SettingsViewModel : BindableObject
{
    private ICommand _manualConnectCommand;
    public ICommand ManualConnectCommand => _manualConnectCommand ??= new Command(ManualConnect);

    private ICommand _autodiscoverCommand;
    public ICommand AutodiscoverCommand => _autodiscoverCommand ??= new Command(Autodiscover);

    private readonly SettingsProvider _settingsProvider;

    public SettingsViewModel(SettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        _hubIpAddress = _settingsProvider.HubIpAddress;
        _darkMode = _settingsProvider.DarkMode;
        _isConnected = !string.IsNullOrEmpty(_hubIpAddress);
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

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
        }
    }

    private bool _darkMode;
    public bool DarkMode
    {
        get => _darkMode;
        set
        {
            _darkMode = value;
            _settingsProvider.SetDarkMode(value);
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

    private void ManualConnect()
    {
        IsConnected = false;
        if (HubIpAddress != "localhost")
        {
            try
            {
                System.Net.IPAddress.Parse(HubIpAddress);
            }
            catch (FormatException)
            {
                ErrorMessage = $"'{HubIpAddress}' is not valid IP address.";
                ShowError = true;
                return;
            }
        }
        _settingsProvider.SetHubAddress(HubIpAddress);
        IsConnected = true;
    }

    private void Autodiscover()
    {
        IsConnected = false;

        HubIpAddress = "localhost";

        _settingsProvider.SetHubAddress(HubIpAddress);
        IsConnected = true;
        ShowError = false;
    }
}

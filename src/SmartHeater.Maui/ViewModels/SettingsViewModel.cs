using System.Net;
using System.Net.NetworkInformation;
using System.Text;

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
        try
        {
            CheckAvailability(IPAddress.Parse(_hubIpAddress));
        }
        catch { }
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
        await Task.Run(() => CheckAvailability(iPAddress));
    }

    private void Autodiscover()
    {
        IsConnected = null;

        HubIpAddress = "192.168.1.242";

        _settingsProvider.SetHubAddress(HubIpAddress);
        IsConnected = true;
        ShowError = false;
    }

    private void CheckAvailability(IPAddress ipAddress)
    {
        var pingSender = new Ping();
        var options = new PingOptions
        {
            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            DontFragment = true
        };
        // Create a buffer of 32 bytes of data to be transmitted.
        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        int timeout = 10;
        try
        {
            PingReply reply = pingSender.Send(ipAddress, timeout, buffer, options);

            if (reply.Status != IPStatus.Success)
            {
                throw new PingException($"Could not connect to {ipAddress}.");
            }
            _settingsProvider.SetHubAddress(ipAddress.ToString());
            IsConnected = true;
            ShowError = false;
        }
        catch (PingException e)
        {
            ErrorMessage = e.Message;
            ShowError = true;
            IsConnected = false;
        }
    }
}

namespace SmartHeater.Hub.Services.Interfaces;

public interface IHeaterControlService
{
    string IPAddress { get; }

    Task TurnOnAsync();
    Task TurnOffAsync();
    Task<HeaterStatusModel?> GetStatusAsync();
}

namespace SmartHeater.Hub.Services.Interfaces;

public interface IHeaterService
{
    string IPAddress { get; }

    Task TurnOnAsync();
    Task TurnOffAsync();
    Task<HeaterStatusModel?> GetStatusAsync();
}

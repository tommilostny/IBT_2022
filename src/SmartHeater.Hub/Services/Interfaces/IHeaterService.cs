namespace SmartHeater.Hub.Services.Interfaces;

public interface IHeaterService
{
    string IPAddress { get; }

    Task TurnOn();
    Task TurnOff();
    Task<HeaterStatusModel?> GetStatus();
}

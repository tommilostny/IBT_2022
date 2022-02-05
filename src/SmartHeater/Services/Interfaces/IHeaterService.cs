namespace SmartHeater.Services.Interfaces;

public interface IHeaterService
{
    string IPAddress { get; }

    Task TurnOn();
    Task TurnOff();
    Task<HeaterStatus> GetStatus();
}

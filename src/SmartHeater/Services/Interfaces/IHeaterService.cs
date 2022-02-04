namespace SmartHeater.Services.Interfaces;

public interface IHeaterService
{
    string IPAddress { get; }

    Task TurnOn();
    Task TurnOff();
    Task<double> ReadTemperature();
    Task<double> ReadPower();
    Task<bool?> GetStatus();
}

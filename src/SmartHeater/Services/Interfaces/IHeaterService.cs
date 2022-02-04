namespace SmartHeater.Services.Interfaces;

public interface IHeaterService
{
    string IPAddress { get; }

    Task TurnOn();
    Task TurnOff();
    Task<double?> ReadTemperature(bool fromCache = false);
    Task<double?> ReadPower(bool fromCache = false);
    Task<bool?> GetStatus(bool fromCache = false);
}

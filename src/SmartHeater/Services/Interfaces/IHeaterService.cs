namespace SmartHeater.Services.Interfaces;

public interface IHeaterService
{
    void TurnOn();
    void TurnOff();
    double ReadTemperature();
}

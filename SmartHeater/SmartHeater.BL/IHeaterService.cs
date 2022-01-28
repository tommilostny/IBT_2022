namespace SmartHeater.BL;

public interface IHeaterService
{
    void TurnOn();
    void TurnOff();
    double ReadTemperature();
}

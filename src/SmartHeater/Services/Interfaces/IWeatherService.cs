namespace SmartHeater.Services.Interfaces;

/// <summary>
/// <b>Warning!</b> At least one of methods (ReadTemperatureC or ReadTemperatureF) needs to be implemented.
/// </summary>
public interface IWeatherService
{
    /// <returns>Temperature in Celsius.</returns>
    double ReadTemperatureC() => ReadTemperatureF() - 32.0 * (5.0 / 9.0);

    /// <returns>Temperature in Fahrenheit.</returns>
    double ReadTemperatureF() => ReadTemperatureC() * (9.0 / 5.0) + 32.0;
}

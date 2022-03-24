namespace SmartHeater.Hub.Services.Interfaces;

public interface IDatabaseService
{
    /// <summary>
    /// Writes measurement to database.
    /// </summary>
    /// <param name="heater">Heater status from <seealso cref="IHeaterService.GetStatusAsync"/></param>
    /// <param name="weather">Outside weather in °C</param>
    /// <returns>String representation of the measurement written to the database.</returns>
    string WriteMeasurement(HeaterStatusModel heater, double? weather);

    Task<IEnumerable<ModelInput>> ReadTemperatureDiffsAsync(HeaterListModel heater);
}

namespace SmartHeater.Models;

public record HeaterStatus(string IPAddress, DateTime MeasurementTime)
{
    public bool? IsTurnedOn { get; init; }

    public double? Temperature { get; init; }

    public double? Power { get; init; }
}

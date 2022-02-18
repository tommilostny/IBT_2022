namespace SmartHeater.Shared.Models;

public record HeaterStatusModel(string IPAddress, DateTime MeasurementTime)
{
    public bool? IsTurnedOn { get; init; }

    public double? Temperature { get; init; }

    public double? Power { get; init; }
}

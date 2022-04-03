namespace SmartHeater.Shared.Models;

public record TemperatureRecordModel
{
    public float Temperature { get; set; }

    public DateTime? MeasurementTime { get; set; }
}

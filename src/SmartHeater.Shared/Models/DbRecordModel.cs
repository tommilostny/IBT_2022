namespace SmartHeater.Shared.Models;

public record DbRecordModel
{
    public float Value { get; set; }

    public DateTime? MeasurementTime { get; set; }
}

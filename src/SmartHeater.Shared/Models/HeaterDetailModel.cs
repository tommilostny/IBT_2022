using SmartHeater.Shared.Enums;

namespace SmartHeater.Shared.Models;

public record HeaterDetailModel(string IpAddress,
                                string Name)
{
    public HeaterTypes? HeaterType { get; set; }

    public float? ReferenceTemperature { get; set; }

    public HeaterStatusModel? LastMeasurement { get; set; }
}

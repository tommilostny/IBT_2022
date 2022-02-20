using SmartHeater.Shared.Enums;

namespace SmartHeater.Shared.Models;

public record HeaterDetailModel(string IpAddress, string Name, HeaterTypes? HeaterType)
{
    public HeaterStatusModel? LastMeasurement { get; set; }
}

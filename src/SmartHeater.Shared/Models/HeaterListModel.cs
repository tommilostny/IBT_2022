using SmartHeater.Shared.Enums;

namespace SmartHeater.Shared.Models;

public record HeaterListModel(string IpAddress,
                              string Name,
                              HeaterTypes HeaterType,
                              float ReferenceTemperature);

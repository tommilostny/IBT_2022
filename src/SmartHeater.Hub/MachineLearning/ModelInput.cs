using Microsoft.ML.Data;

namespace SmartHeater.Hub.MachineLearning;

/// <summary>
/// model input class for SmartHeaterModel.
/// </summary>
public class ModelInput
{
    [ColumnName(@"temperatureDiff")]
    public float TemperatureDiff { get; set; }

}

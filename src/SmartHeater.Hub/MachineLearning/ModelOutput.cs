using Microsoft.ML.Data;

namespace SmartHeater.Hub.MachineLearning;

#nullable disable

/// <summary>
/// model output class for SmartHeaterModel.
/// </summary>
public class ModelOutput
{
    [ColumnName(@"temperatureDiff")]
    public float[] TemperatureDiff { get; set; }

    [ColumnName(@"temperatureDiff_LB")]
    public float[] TemperatureDiff_LB { get; set; }

    [ColumnName(@"temperatureDiff_UB")]
    public float[] TemperatureDiff_UB { get; set; }

}

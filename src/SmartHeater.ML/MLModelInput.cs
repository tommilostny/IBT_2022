using Microsoft.ML.Data;

namespace SmartHeater.ML;

/// <summary>
/// model input class for SmartHeaterModel.
/// </summary>
public class MLModelInput
{
    [ColumnName(@"temperatureDiff")]
    [LoadColumn(fieldIndex: 1)]
    public float TemperatureDiff { get; set; }
}

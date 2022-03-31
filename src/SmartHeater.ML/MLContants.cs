namespace SmartHeater.Shared.Static;

internal static class MLContants
{
    public const string DefaultModelFilePath = "SmartHeaterModel.zip";
    public const string TrainingFileName = "training.csv";
    public const string TestingFileName = "testing.csv";

    public const int TrainingStartYear = 2018;
    public const int TrainingEndYear = 2020;

    public const double TrainingStartRoomTemperature = 18.5;

    public const int Horizon = 10;
}

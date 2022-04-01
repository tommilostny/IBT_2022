namespace SmartHeater.Shared.Static;

public static class DbFields
{
    public const string Temperature = "temperature";
    public const string Power = "power";
    public const string Weather = "weather";

    public const string MeasurementName = "heater_status";
    public const string HeaterTag = "heater";

    public static bool IsValid(string value) => GetAll().Contains(value);

    public static IEnumerable<string> GetAll()
    {
        yield return Temperature;
        yield return Power;
        yield return Weather;
    }
}

namespace SmartHeater.Shared.Static;

public static class HistoryPeriods
{
    public const string Minute1 = "1m";
    public const string Minutes5 = "5m";
    public const string Minutes15 = "15m";
    public const string Minutes30 = "30m";
    public const string Minutes45 = "45m";
    
    public const string Hour1 = "1h";
    public const string Hours2 = "2h";
    public const string Hours3 = "3h";
    public const string Hours6 = "6h";
    public const string Hours12 = "12h";
    public const string Hours24 = "24h";

    public const string Days2 = "2d";
    public const string Days7 = "7d";
    public const string Days14 = "14d";
    public const string Days30 = "30d";

    public static bool IsValid(string value) => GetAll().Contains(value);

    public static IEnumerable<string> GetAll()
    {
        yield return Minute1;
        yield return Minutes5;
        yield return Minutes15;
        yield return Minutes30;
        yield return Minutes45;
        yield return Hour1;
        yield return Hours2;
        yield return Hours3;
        yield return Hours6;
        yield return Hours12;
        yield return Hours24;
        yield return Days2;
        yield return Days7;
        yield return Days14;
        yield return Days30;
    }

    public static string? AggregationWindow(string period) => period switch
    {
        Minute1 => "1s",
        Minutes5 or Minutes15 or Minutes30 or Minutes45 or Hour1 => "10s",
        Hours2 or Hours3 or Hours6 => "1m",
        Hours12 => "2m",
        Hours24 => "4m",
        Days2 => "10m",
        Days7 => "30m",
        Days14 => "45m",
        Days30 => "1h",
        _ => null
    };
}

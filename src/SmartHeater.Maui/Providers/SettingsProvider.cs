namespace SmartHeater.Maui.Providers;

public class SettingsProvider
{
    [JsonIgnore]
    public string HubUri => $"http://{HubIpAddress}";

    public string HubIpAddress { get; set; } = string.Empty;

    public void SetHubAddress(string ipAddress)
    {
        HubIpAddress = ipAddress;
        SaveToJson();
    }

    public static SettingsProvider LoadFromJson()
    {
        var filePath = SettingsJsonFilePath();
        if (File.Exists(filePath))
        {
            var jsonStr = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<SettingsProvider>(jsonStr);
        }
        return new();
    }

    private void SaveToJson()
    {
        var jsonStr = JsonConvert.SerializeObject(this);
        File.WriteAllText(SettingsJsonFilePath(), jsonStr);
    }

    private static string SettingsJsonFilePath()
    {
        var localDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsJsonFile = "settings.json";
        return Path.Combine(localDir, settingsJsonFile);
    }
}

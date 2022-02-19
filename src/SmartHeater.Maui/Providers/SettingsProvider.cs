﻿namespace SmartHeater.Maui.Providers;

public class SettingsProvider
{
    [JsonIgnore]
    public string HubUri => $"https://{HubIpAddress}:7232";

    public string HubIpAddress { get; set; } = string.Empty;

    public bool DarkMode { get; set; } = false;

    public void SetHubAddress(string ipAddress)
    {
        HubIpAddress = ipAddress;
        SaveToJson();
    }

    public void SetDarkMode(bool darkMode)
    {
        DarkMode = darkMode;
        SaveToJson();
    }

    public static SettingsProvider LoadFromJson()
    {
        var filePath = SettingsJsonFilePath();
        if (File.Exists(filePath))
        {
            var jsonStr = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<SettingsProvider>(jsonStr);
        }
        return new();
    }

    private void SaveToJson()
    {
        var jsonStr = JsonSerializer.Serialize(this);
        File.WriteAllText(SettingsJsonFilePath(), jsonStr);
    }

    private static string SettingsJsonFilePath()
    {
        var settingsJsonFile = "settings.json";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), settingsJsonFile);
    }
}

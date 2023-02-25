using System.IO;
using System.Text.Json;
using Apollo.IO;

namespace Apollo;

/// <summary>
/// Class passed to Json Deserializer to store settings
/// </summary>
public class StoredSettings
{
    public static readonly string SettingsPath = "settings.json";
    public static readonly string ProfilesPath = "states";
    public string SelectedProfileName { get; set; }
    public int DefaultLength { get; set; }

    public static StoredSettings Default()
    {
        var settings = new StoredSettings();
        // name of the directory in "states" directory which contains the profile data
        settings.SelectedProfileName = "default";
        settings.DefaultLength = 10000;

        return settings;
    }

    /// <summary>
    /// Generate StoredSettings object from JSON file 
    /// </summary>
    /// <returns>StoredSettings object containing data in JSON file</returns>
    public static StoredSettings Load()
    {
        var str = File.ReadAllText(SettingsPath);
        return JsonSerializer.Deserialize<StoredSettings>(str);
    }

    public void Save()
    {
        var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(SettingsPath, jsonString);
    }
}
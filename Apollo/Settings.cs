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
    public string SelectedProfilePath { get; set; }
    public int DefaultLength { get; set; }

    public static StoredSettings Default()
    {
        var settings = new StoredSettings();
        // name of the directory in "states" directory which contains the profile data
        settings.SelectedProfilePath = "default";
        settings.DefaultLength = 10000;

        return settings;
    }
}
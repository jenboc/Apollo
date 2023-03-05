using System.IO;
using System.Text.Json;
using Apollo.IO;

namespace Apollo;

/// <summary>
/// Class passed to Json Deserializer to store settings
/// </summary>
public class StoredSettings
{
    public const string SETTINGS_PATH = "settings.json";
    public string SelectedProfileName { get; set; } // Currently selected state profile
    public string ProfilesPath { get; set; } // Path where all the state profiles are stored
    public string LogsPath { get; set; } // Path where the logs are saved
    public int MinEpochs { get; set; } // Default Minimum Epochs [TRAINING SETTINGS] 
    public int MaxEpochs { get; set; } // Default Maximum Epochs [TRAINING SETTINGS]
    public float MaxError { get; set; } // Default Maximum Error [TRAINING SETTINGS]
    public int BatchesPerEpoch { get; set; } // Default Batches per Epoch [TRAINING SETTINGS]
    public int GenerationLength { get; set; } // Default Generation Length [GENERATION SETTINGS]
    public int Bpm { get; set; } // Default Beats per Minute [GENERATION SETTINGS]

    public StoredSettings(string selectedProfile, string profilesPath, string logsPath, int minEpochs, int maxEpochs,
        float maxError, int batchesPerEpoch, int generationLength, int bpm)
    {
        SelectedProfileName = selectedProfile;
        ProfilesPath = profilesPath;
        LogsPath = logsPath;
        MinEpochs = minEpochs;
        MaxEpochs = maxEpochs;
        MaxError = maxError;
        BatchesPerEpoch = batchesPerEpoch;
        GenerationLength = generationLength;
        Bpm = bpm;
    }
    
    /// <summary>
    /// Get default settings
    /// </summary>
    public static StoredSettings Default()
    {
        return new StoredSettings("default", "states", "logs", 100,
            200, 0.05f, 50, 10000, 30);
    }

    /// <summary>
    /// Generate StoredSettings object from JSON file 
    /// </summary>
    /// <returns>StoredSettings object containing data in JSON file</returns>
    public static StoredSettings Load()
    {
        var str = File.ReadAllText(SETTINGS_PATH);
        return JsonSerializer.Deserialize<StoredSettings>(str);
    }

    public void Save()
    {
        var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(SETTINGS_PATH, jsonString);
    }
}
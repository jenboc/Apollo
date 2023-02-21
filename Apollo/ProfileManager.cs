using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using Apollo.IO;
using Microsoft.Win32;

namespace Apollo;

/// <summary>
/// Class which handles the creation and switching between profiles
/// </summary>
public class ProfileManager
{
    private readonly Dictionary<string, Profile> _profiles;
    private string ProfilesPath { get; set; }

    /// <summary>
    /// Instantiate a ProfileManager object
    /// </summary>
    /// <param name="profilesPath">The path where all the profiles are stored</param>
    public ProfileManager(string profilesPath)
    {
        _profiles = new Dictionary<string, Profile>();
        ProfilesPath = profilesPath; 
        LoadFromFileSystem(); // Fill the dictionary with already existing profiles 
    }

    /// <summary>
    /// Opens a file dialog for the user to select multiple files
    /// </summary>
    /// <returns>A list of the paths to the selected training files</returns>
    private string[] SelectTrainingFiles(bool mandatory=false)
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Multiselect = true;
        fileDialog.Title = "Select multiple training files";
        fileDialog.Filter = "MIDI File|*.mid";

        if (fileDialog.ShowDialog() == true)
        {
            return fileDialog.FileNames;
        }
        
        if (mandatory) 
            throw new Exception("You cannot use the program without selecting the training files");

        return Array.Empty<string>();
    }
    
    /// <summary>
    /// Loads existing profiles from the file system and adds them to the dictionary
    /// </summary>
    private void LoadFromFileSystem()
    {
        if (!Directory.Exists(ProfilesPath))
            Directory.CreateDirectory(ProfilesPath);
        
        var profileDirectories = Directory.GetDirectories(ProfilesPath);

        if (profileDirectories.Length == 0)
        {
            CreateProfile("default", mandatory: true);
            profileDirectories = Directory.GetDirectories(ProfilesPath);
        }

        // Check for valid profiles (profiles with a schema.json file) 
        foreach (var dir in profileDirectories)
        {
            var schemaPath = Path.Join(dir, "schema.json");

            if (!File.Exists(schemaPath)) // Continue if invalid 
                continue;
            
            // Read the schema + add to dictionary
            var profile = ReadJson<Profile>(schemaPath);
            LogManager.WriteLine(profile.TrainingDataDirectory);
            _profiles.Add(dir, profile);
        }
    }

    /// <summary>
    /// Create a profile
    /// </summary>
    public void CreateProfile(string name, bool mandatory=false)
    {
        var trainingFiles = SelectTrainingFiles(mandatory);

        if (trainingFiles.Length == 0) // Do not continue if no files are selected 
        {
            MessageBox.Show("You must select training files in order to create a profile");
            return;
        }

        var path = name; 
        var profile = Profile.Default(path);
        _profiles.Add(name, profile);
    }

    /// <summary>
    /// Load a profile from the profile manager
    /// </summary>
    /// <param name="name">The name of the profile to search for</param>
    /// <returns>The queried profile if it exists, otherwise null</returns>
    public Profile? LoadProfile(string name)
    {
        return _profiles.TryGetValue(name, out var profile) ? profile : null;
    }
    
    private T ReadJson<T>(string path)
    {
        var str = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(str);
    }
    
    private void WriteJson<T>(T obj, string path)
    {
        var jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(path, jsonString);
    }
}
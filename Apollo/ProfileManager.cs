using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
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

    public string[] ProfileNames => _profiles.Keys.ToArray();
    public bool ProfileExists(string name) => _profiles.ContainsKey(name);

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
            MessageBox.Show("No training profiles found. Please close this box to create one.");
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
            _profiles.Add(Path.GetFileName(dir), profile);
        }
    }

    /// <summary>
    /// Create a profile
    /// </summary>
    public void CreateProfile(string name, bool mandatory=false)
    {
        Mouse.SetCursor(Cursors.Wait);
        // Get user to select training files
        var trainingFiles = SelectTrainingFiles(mandatory);
        
        // Do not continue if no files are selected 
        if (trainingFiles.Length == 0) 
        {
            MessageBox.Show("You must select training files in order to create a profile");
            return;
        }
        
        // Create directory where profile info is stored
        var profilePath = Path.Join(ProfilesPath, name);
        Directory.CreateDirectory(profilePath);
        
        // Create the profile and add it to the dictionary
        var profile = Profile.Default(profilePath);
        _profiles.Add(name, profile);
        
        // Copy the training data into the profile training data directory
        Directory.CreateDirectory(profile.TrainingDataDirectory);
        foreach (var file in trainingFiles)
        {
            var currentPath = Path.GetFullPath(file);
            var fileName = Path.GetFileName(file);
            var newPath = Path.Join(profile.TrainingDataDirectory, fileName);
            
            File.Copy(currentPath, newPath);
        }
        
        // Create schema.json file 
        var schemaPath = Path.Join(profilePath, "schema.json");
        WriteJson(profile, schemaPath);
        Mouse.SetCursor(null);
    }

    /// <summary>
    /// Retrieve a profile from the profile manager
    /// </summary>
    /// <param name="name">The name of the profile to search for</param>
    /// <returns>The queried profile if it exists, otherwise null</returns>
    public Profile? GetProfile(string name)
    {
        return _profiles.TryGetValue(name, out var profile) ? profile : null;
    }
    
    /// <summary>
    /// Read a JSON file
    /// </summary>
    /// <param name="path">Path to JSON file</param>
    /// <typeparam name="T">Model class type to store the data</typeparam>
    private T ReadJson<T>(string path)
    {
        var str = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(str);
    }
    
    /// <summary>
    /// Write to A JSON file
    /// </summary>
    /// <param name="obj">The object you want to write the data from</param>
    /// <param name="path">The path to the JSON file</param>
    /// <typeparam name="T">The class type which you are writing the data from</typeparam>
    private void WriteJson<T>(T obj, string path)
    {
        var jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(path, jsonString);
    }
}
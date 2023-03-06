using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Apollo.IO;
using Apollo.NeuralNet;
using Microsoft.Win32;

namespace Apollo;

/// <summary>
///     Class which handles the creation and switching between profiles
/// </summary>
public class ProfileManager
{
    private readonly Dictionary<string, Profile> _profiles;

    /// <summary>
    ///     Instantiate a ProfileManager object
    /// </summary>
    /// <param name="profilesPath">The path where all the profiles are stored</param>
    public ProfileManager(string profilesPath)
    {
        _profiles = new Dictionary<string, Profile>();
        ProfilesPath = profilesPath;
        LoadFromFileSystem(); // Fill the dictionary with already existing profiles 
    }

    private string ProfilesPath { get; set; } // The path where all profiles will be found 

    public string[] ProfileNames => _profiles.Keys.ToArray(); // Getter for array of dictionary keys

    /// <summary>
    ///     Check if a profile with a name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <returns>Boolean depicting whether a profile with the passed name exists</returns>
    public bool ProfileExists(string name)
    {
        return _profiles.ContainsKey(name);
    }

    /// <summary>
    ///     Returns a profile from the dictionary
    /// </summary>
    /// <returns></returns>
    public Profile Any()
    {
        return _profiles.Values.ToArray()[0];
    }

    public void ChangeProfilesPath(string newPath)
    {
        ProfilesPath = newPath;
    }

    /// <summary>
    ///     Opens a file dialog for the user to select multiple files
    /// </summary>
    /// <returns>A list of the paths to the selected training files</returns>
    private string[] SelectTrainingFiles(bool mandatory = false)
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Multiselect = true;
        fileDialog.Title = "Select multiple training files";
        fileDialog.Filter = "MIDI File|*.mid;*.midi";

        if (fileDialog.ShowDialog() == true) return fileDialog.FileNames;

        if (mandatory)
            throw new Exception("You cannot use the program without selecting the training files");

        return Array.Empty<string>();
    }

    /// <summary>
    ///     Loads existing profiles from the file system and adds them to the dictionary.
    ///     If no profiles are found, then it will create one.
    /// </summary>
    private void LoadFromFileSystem()
    {
        // If the root profile path doesn't exist, create it 
        if (!Directory.Exists(ProfilesPath))
            Directory.CreateDirectory(ProfilesPath);

        var profileDirectories = Directory.GetDirectories(ProfilesPath);

        // Create default profile if no profiles exist
        if (profileDirectories.Length == 0)
        {
            MessageBox.Show("No training profiles found. Please close this box to create one.");
            CreateProfile("default", true);
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

            // Add to dictionary if it doesn't already exist
            // Necessary since if default directory is created here, then it will already be in dictionary 
            if (!_profiles.ContainsKey(Path.GetFileName(dir)))
                _profiles.Add(Path.GetFileName(dir), profile);
        }
    }

    /// <summary>
    ///     Create a profile
    /// </summary>
    public void CreateProfile(string name, bool mandatory = false)
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
        profile.Vocab = GetVocabList(profile);
        var schemaPath = Path.Join(profilePath, "schema.json");
        WriteJson(profile, schemaPath);
        Mouse.SetCursor(null);
    }

    private string GetVocabList(Profile profile)
    {
        var vocabList = new Vocab();

        var stringReps = MidiManager.ReadDir(profile.TrainingDataDirectory);

        Parallel.ForEach(stringReps, rep => { vocabList.AddCharacters(rep.ToCharArray()); });

        return vocabList.AsString();
    }

    /// <summary>
    ///     Retrieve a profile from the profile manager
    /// </summary>
    /// <param name="name">The name of the profile to search for</param>
    /// <returns>The queried profile if it exists, otherwise null</returns>
    public Profile? GetProfile(string name)
    {
        return _profiles.TryGetValue(name, out var profile) ? profile : null;
    }

    /// <summary>
    ///     Delete a profile
    /// </summary>
    /// <param name="name">Name of the profile to delete</param>
    public void DeleteProfile(string name)
    {
        // Do not continue if the profile does not exist
        if (!ProfileExists(name))
            return;

        // Get the directory of the profile 
        var profilePath = Path.Join(ProfilesPath, name);

        // Delete directory and its contents
        Directory.Delete(profilePath, true);
    }

    /// <summary>
    ///     Read a JSON file
    /// </summary>
    /// <param name="path">Path to JSON file</param>
    /// <typeparam name="T">Model class type to store the data</typeparam>
    private T ReadJson<T>(string path)
    {
        var str = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(str);
    }

    /// <summary>
    ///     Write to A JSON file
    /// </summary>
    /// <param name="obj">The object you want to write the data from</param>
    /// <param name="path">The path to the JSON file</param>
    /// <typeparam name="T">The class type which you are writing the data from</typeparam>
    private void WriteJson<T>(T obj, string path)
    {
        var jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, jsonString);
    }
}
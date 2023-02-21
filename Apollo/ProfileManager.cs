using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;

namespace Apollo;

/// <summary>
/// Class which handles the creation and switching between profiles
/// </summary>
public class ProfileManager
{
    private readonly Dictionary<string, Profile> _profiles;

    public ProfileManager()
    {
        _profiles = new Dictionary<string, Profile>();
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
        
    }

    /// <summary>
    /// Create a profile
    /// </summary>
    /// <param name="name"></param>
    public void CreateProfile(string name)
    {
        var trainingFiles = SelectTrainingFiles();

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
}
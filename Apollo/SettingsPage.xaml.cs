﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Apollo.IO;
using Apollo.NeuralNet;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Apollo;

public partial class SettingsPage : Page
{
    /// <summary>
    /// Objects which are used throughout the page
    /// </summary>
    private ProfileManager ProfileManagement { get; }
    private NeuralNetwork Network { get; }
    private StoredSettings Settings { get; }
    public SettingsPage()
    {
        InitializeComponent();
        
        // Retrieve objects from App.xaml.cs (AGGREGATION) 
        ProfileManagement = (Application.Current as App).ProfileManagement;
        Network = (Application.Current as App).Network;
        Settings = (Application.Current as App).Settings;
        
        // Load the existing profiles as options in the combo box and select the currently selected one 
        AddProfilesToComboBox(); 
        
        // Change all labels/sliders to current settings
        LogPathLabel.Content = Settings.LogsPath;
        ProfilePathLabel.Content = Settings.ProfilesPath;
        MinEpochSlider.Value = Settings.MinEpochs;
        MaxEpochSlider.Value = Settings.MaxEpochs;
        MaxErrorSlider.Value = Settings.MaxError;
        BatchesPerEpochSlider.Value = Settings.BatchesPerEpoch;
        GenerationLenSlider.Value = Settings.GenerationLength;
        BpmSlider.Value = Settings.Bpm;
    }

    #region Helper Subroutines 
    
    /// <summary>
    /// Procedure which adds profiles already in the ProfileManager to the combo box 
    /// </summary>
    private void AddProfilesToComboBox()
    {
        for (var i = 0; i < ProfileManagement.ProfileNames.Length; i++)
        {
            var profileName = ProfileManagement.ProfileNames[i];
            ProfileComboBox.Items.Add(profileName);
            
            // Will never be a null reference, we are retrieving profileName from the list of keys of the dictionary
            // If it is the selected profile, set it as the selected option in the combo box
            if (Network.CurrentProfile == ProfileManagement.GetProfile(profileName))
                ProfileComboBox.SelectedIndex = i;
        }
    }

    /// <summary>
    /// Change the profile across all objects used in the application
    /// </summary>
    /// <param name="profileName">The name of the profile that was selected</param>
    /// <param name="changeComboBox">Whether to change the currently selected value of the combo box</param>
    private void ChangeProfile(string profileName, bool changeComboBox=false)
    {
        // Retrieve profile from ProfileManager
        var profile = ProfileManagement.GetProfile(profileName);

        // Do not continue if the profile is not found 
        if (object.ReferenceEquals(profile, null))
            return; 
        
        // Change the profile used in the NeuralNetwork object
        Network.ChangeProfile(profile);

        // Change combo box if required
        if (changeComboBox)
        {
            var comboIndex = ProfileComboBox.Items.IndexOf(profileName);
            ProfileComboBox.SelectedIndex = comboIndex;
        }

        // Change settings so that the selected profile changes on start up 
        Settings.SelectedProfileName = profileName;
        Settings.Save();
    }

    
    /// <summary>
    /// Delete a profile from the file system
    /// </summary>
    private void DeleteProfile(string profileName)
    {
        ProfileManagement.DeleteProfile(profileName); // Delete from file system
        ProfileComboBox.Items.Remove(profileName); // Remove from ComboBox
    }

    /// <summary>
    /// Move a folder to a new destination
    /// </summary>
    /// <param name="oldPath">The path to the directory you wish to move</param>
    /// <param name="newPath">The path to where you wish to move the directory</param>
    private void MoveFolder(string oldPath, string newPath)
    {
        Directory.CreateDirectory(newPath);
        
        var files = Directory.GetFiles(oldPath);

        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var newFilePath = Path.Join(newPath, filename);
            
            File.Copy(file, newFilePath);
            File.Delete(file);
        }

        var dirs = Directory.GetDirectories(oldPath);
        foreach (var dir in dirs)
        {
            var dirname = Path.GetFileName(dir);
            var newDir = Path.Join(newPath, dirname);
            MoveFolder(dir, newDir);
        }
    }
    
    #endregion
    
    #region Events

    /// <summary>
    /// Event which is called when the Create Profile button is clicked
    /// </summary>
    private void OnCreateButtonClicked(object sender, RoutedEventArgs e)
    {
        // Retrieve the name via a prompt 
        var name = OpenNamePrompt();

        // Do not continue if no name was provided
        if (name == "")
        {
            MessageBox.Show("Must enter a profile name in order to create a profile");
            return;
        }

        // If it already exists do not continue 
        if (ProfileManagement.ProfileExists(name))
        {
            MessageBox.Show("A profile already exists with that name");
            return;
        }

        // Go through necessary steps to create the profile 
        ProfileManagement.CreateProfile(name);

        ProfileComboBox.Items.Add(name);

        // Change the profile and value in the combo box (since it was not selected from the combo box) 
        ChangeProfile(name, changeComboBox: true); 
    }

    /// <summary>
    /// Event which is called when the Delete Profile button is clicked
    /// </summary>
    private void OnDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        var selectedIndex = ProfileComboBox.SelectedIndex;
        var selectedName = (string)ProfileComboBox.Items.GetItemAt(selectedIndex);

        DeleteProfile(selectedName);
    }

    /// <summary>
    /// Event which is called when the Logs Save Path change button is clicked
    /// </summary>
    private void OnLogChangeClick(object sender, RoutedEventArgs e)
    {
        // Get path from file dialog 
        var path = GetPath(Settings.LogsPath);

        if (string.IsNullOrEmpty(path))
            return;
        
        // Move logs that are already there
        MoveFolder(Settings.LogsPath, path);
        
        // Change the path 
        Settings.LogsPath = path;
        LogManager.ChangeLogPath(Settings.LogsPath);
    }
    
    /// <summary>
    /// Event which is called when the Logs Save Path change button is clicked
    /// </summary>
    private void OnProfilePathChangeClick(object sender, RoutedEventArgs e)
    {
        // Get path from file dialog 
        var path = GetPath(Settings.ProfilesPath);

        if (string.IsNullOrEmpty(path))
            return;
        
        // Move existent profiles to new location
        MoveFolder(Settings.ProfilesPath, path);
        
        // Change the path
        Settings.ProfilesPath = path;
        ProfileManagement.ChangeProfilesPath(Settings.ProfilesPath);
    }

    /// <summary>
    /// Event which is called when the min epochs slider is changed
    /// </summary>
    private void OnMinEpochChange(object sender, RoutedEventArgs e)
    {
        var newEpochs = (int)MinEpochSlider.Value;
        Settings.MinEpochs = newEpochs;
        
        // Adjust minimum of max epochs so that min epochs > max epochs 
        // Max Epochs minimum can never be lower than 100
        MaxEpochSlider.Minimum = (newEpochs + 1 > 100) ? newEpochs + 1 : 100; 
    }

    /// <summary>
    /// Event which is called when the max epoch slider is changed
    /// </summary>
    private void OnMaxEpochChange(object sender, RoutedEventArgs e)
    {
        var newEpochs = (int)MaxEpochSlider.Value;
        Settings.MaxEpochs = newEpochs;

        MinEpochSlider.Maximum = newEpochs - 1;
    }

    /// <summary>
    /// Event which is called when the max error slider is changed
    /// </summary>
    private void OnMaxErrorChange(object sender, RoutedEventArgs e)
    {
        var newError = (float)MaxErrorSlider.Value;
        Settings.MaxError = newError;
    }

    /// <summary>
    /// Event which is called when the batches per epoch slider is changed
    /// </summary>
    private void OnBatchesPerEpochChange(object sender, RoutedEventArgs e)
    {
        var newBatches = (int)BatchesPerEpochSlider.Value;
        Settings.BatchesPerEpoch = newBatches;
    }

    /// <summary>
    /// Event which is called when the characters to generate slider is changed
    /// </summary>
    private void OnGenLengthChange(object sender, RoutedEventArgs e)
    {
        var newLength = (int)GenerationLenSlider.Value;
        Settings.GenerationLength = newLength;
    }

    /// <summary>
    /// Event which is called when the bpm slider is changed
    /// </summary>
    private void OnBpmChange(object sender, RoutedEventArgs e)
    {
        var newBpm = (int)BpmSlider.Value;
        Settings.Bpm = newBpm;
    }

    /// <summary>
    /// Event which is called when the combo box's selected value changed
    /// </summary>
    private void OnProfileSelected(object sender, RoutedEventArgs e)
    {
        var selectedIndex = ProfileComboBox.SelectedIndex;
        var selectedName = (string)ProfileComboBox.Items.GetItemAt(selectedIndex);

        ChangeProfile(selectedName);
    }
    
    #endregion

    #region Prompts + Dialogs
    
    /// <summary>
    /// Function which opens a text dialog and returns the entered value 
    /// </summary>
    /// <returns>The value entered into the prompt</returns>
    private string OpenNamePrompt()
    {
        var dialog = new TextDialog("Enter Profile Name:");

        if (dialog.ShowDialog() == true)
            return dialog.Value;

        return "";
    }

    /// <summary>
    /// Open a File Dialog for the user to select a directory path
    /// </summary>
    /// <returns>The path selected</returns>
    private string GetPath(string currentDirectory)
    {
        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = currentDirectory;
        dialog.IsFolderPicker = true;

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            return dialog.FileName;
        }

        return string.Empty;
    }
    
    #endregion
}
using System.Windows;
using System.Windows.Controls;
using Apollo.NeuralNet;

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
        
        // Load the existing profiles as options in the combo box
        AddProfilesToComboBox(); 
    }

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
    /// Event which is called when the combo box's selected value changed
    /// </summary>
    private void OnProfileSelected(object sender, RoutedEventArgs e)
    {
        var selectedIndex = ProfileComboBox.SelectedIndex;
        var selectedName = (string)ProfileComboBox.Items.GetItemAt(selectedIndex);

        ChangeProfile(selectedName);
    }

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
}
using System.Windows;
using System.Windows.Controls;
using Apollo.NeuralNet;

namespace Apollo;

public partial class SettingsPage : Page
{
    private ProfileManager _profileManager;
    private NeuralNetwork _network; 
    public SettingsPage()
    {
        InitializeComponent();
        _profileManager = (Application.Current as App).ProfileManagement;
        _network = (Application.Current as App).Network;
        
        AddProfilesToComboBox(); 
    }

    private void AddProfilesToComboBox()
    {
        for (var i = 0; i < _profileManager.ProfileNames.Length; i++)
        {
            var profileName = _profileManager.ProfileNames[i];
            ProfileComboBox.Items.Add(profileName);

            if (_network.CurrentProfile == _profileManager.GetProfile(profileName))
                ProfileComboBox.SelectedIndex = i;
        }
    }

    private void OnCreateButtonClicked(object sender, RoutedEventArgs e)
    {
        var name = OpenNamePrompt();

        if (name == "")
        {
            MessageBox.Show("Must enter a profile name in order to create a profile");
            return;
        }

        if (_profileManager.ProfileExists(name))
        {
            MessageBox.Show("A profile already exists with that name");
            return;
        }

        _profileManager.CreateProfile(name);
        ProfileComboBox.Items.Add(name);
        _network.ChangeProfile(_profileManager.GetProfile(name)); 
    }

    private void OnProfileSelected(object sender, RoutedEventArgs e)
    {
        var selectedIndex = ProfileComboBox.SelectedIndex;
        var selectedName = (string)ProfileComboBox.Items.GetItemAt(selectedIndex);
        var selectedProfile = _profileManager.GetProfile(selectedName); 
        
        _network.ChangeProfile(selectedProfile);
    }

    private string OpenNamePrompt()
    {
        var dialog = new TextDialog("Enter Profile Name:");

        if (dialog.ShowDialog() == true)
            return dialog.Value;

        return "";
    }
}
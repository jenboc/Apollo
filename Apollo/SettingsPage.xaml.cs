using System.Windows;
using System.Windows.Controls;

namespace Apollo;

public partial class SettingsPage : Page
{
    private ProfileManager _profileManager; 
    public SettingsPage()
    {
        InitializeComponent();
        _profileManager = (Application.Current as App).ProfileManagement;
        
        AddProfilesToComboBox(); 
    }

    private void AddProfilesToComboBox()
    {
        var network = (Application.Current as App).Network;

        for (var i = 0; i < _profileManager.ProfileNames.Length; i++)
        {
            var profileName = _profileManager.ProfileNames[i];
            ProfileComboBox.Items.Add(profileName);

            if (network.CurrentProfile == _profileManager.GetProfile(profileName))
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
        }

        _profileManager.CreateProfile(name);
        ProfileComboBox.Items.Add(name);
    }

    private void OnProfileSelected(object sender, RoutedEventArgs e)
    {
    }

    private string OpenNamePrompt()
    {
        var dialog = new TextDialog("Enter Profile Name:");

        if (dialog.ShowDialog() == true)
            return dialog.Value;

        return "";
    }
}
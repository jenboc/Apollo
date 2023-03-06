using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Apollo;

public partial class MainWindow : Window
{
    public MainWindow(string startingPage)
    {
        InitializeComponent();

        // Open on the correct page
        CurrentlySelected = TrainButton;

        var buttonToHighlight = TrainButton;
        switch (startingPage.ToLower())
        {
            case "train":
                buttonToHighlight = TrainButton;
                break;
            case "create":
                buttonToHighlight = CreateButton;
                break;
            case "listen":
                buttonToHighlight = ListenButton;
                break;
            case "settings":
                buttonToHighlight = SettingsButton;
                break;
        }

        ChangePage(buttonToHighlight);

        Mouse.OverrideCursor = null; // Change the mouse back to default
    }

    #region Page Selection

    private Button CurrentlySelected { get; set; }

    private readonly SolidColorBrush _selectedColour =
        new((Color)ColorConverter.ConvertFromString("#f25c05"));

    private readonly SolidColorBrush _unselectedColour =
        new((Color)ColorConverter.ConvertFromString("#eaf205"));

    private void ChangePage(Button newSelected)
    {
        // Change window title
        Title = $"Apollo - {newSelected.Content}";

        // Highlight text
        CurrentlySelected.Foreground = _unselectedColour;
        newSelected.Foreground = _selectedColour;
        CurrentlySelected = newSelected;

        // Change page
        switch (newSelected.Name)
        {
            case "TrainButton":
                PageFrame.Source = new Uri("TrainingPage.xaml", UriKind.Relative);
                break;
            case "CreateButton":
                PageFrame.Source = new Uri("CreationPage.xaml", UriKind.Relative);
                break;
            case "ListenButton":
                PageFrame.Source = new Uri("ListenPage.xaml", UriKind.Relative);
                break;
            case "SettingsButton":
                PageFrame.Source = new Uri("SettingsPage.xaml", UriKind.Relative);
                break;
        }
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
        var clickedButton = (Button)sender;
        ChangePage(clickedButton);
    }

    #endregion
}
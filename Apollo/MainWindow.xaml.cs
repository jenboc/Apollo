using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Apollo;

public partial class MainWindow : Window
{
    private Button CurrentlySelected { get; set; }
    
    private readonly SolidColorBrush _selectedColour =
        new ((Color)ColorConverter.ConvertFromString("#f25c05"));
    
    private readonly SolidColorBrush _unselectedColour = 
            new ((Color)ColorConverter.ConvertFromString("#eaf205"));
    
    public MainWindow(string startingState)
    {
        InitializeComponent();

        CurrentlySelected = TrainButton;

        var buttonToHighlight = TrainButton; 
        switch (startingState.ToLower())
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
    }

    private void ChangePage(Button newSelected)
    {
        // Change window title
        Title = $"Apollo - {newSelected.Content}";
        
        // Highlight text
        CurrentlySelected.Foreground = _unselectedColour;
        newSelected.Foreground = _selectedColour;
        CurrentlySelected = newSelected;
        
        // Change page
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
        var clickedButton = (Button)sender;
        ChangePage(clickedButton);
    }
}
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Apollo;

public partial class MainWindow : Window
{
    private States CurrentState { get; set; }
    private Dictionary<States, Label> StateLabels { get; set; }

    private readonly SolidColorBrush _selectedColour =
        new ((Color)ColorConverter.ConvertFromString("#f25c05"));
    
    private readonly SolidColorBrush _unselectedColour = 
            new ((Color)ColorConverter.ConvertFromString("#eaf205"));
    
    public MainWindow(States openingState)
    {
        InitializeComponent();

        CurrentState = States.Train;
        StateLabels = new Dictionary<States, Label>()
        {
            {States.Train, TrainLabel},
            {States.Create, CreateLabel},
            {States.Listen, ListenLabel},
            {States.Settings, SettingsLabel}
        };
        
        ChangePage(openingState); 
    }

    private void ChangePage(States newState)
    {
        // Change label
        var oldHighlighted = StateLabels[CurrentState];
        var newHighlighted = StateLabels[newState];

        oldHighlighted.Foreground = _unselectedColour;
        newHighlighted.Foreground = _selectedColour;

        CurrentState = newState;
        
        // Change page
    }
}
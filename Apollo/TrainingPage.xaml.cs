using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Apollo;

public partial class TrainingPage : Page
{
    public TrainingPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Collect values from slider and send them to neural network
    /// </summary>
    private void TrainButtonClicked(object sender, RoutedEventArgs e)
    {
        var minEpochs = Convert.ToInt32(MinEpochSlider.Value);
        var maxEpochs = Convert.ToInt32(MaxEpochSlider.Value);

        // minEpochs cannot be bigger than maxEpochs
        // Avoid training if this is the case
        if (minEpochs > maxEpochs)
        {
            MessageBox.Show("Minimum Epochs cannot be greater than Maximum Epochs", "Invalid Data Entry",
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }
        
        var maxError = (float)MaxEpochSlider.Value;
        var batchesPerEpoch = Convert.ToInt32(BatchesPerEpochSlider.Value);

        var window = (MainWindow)Window.GetWindow(this); // Page is only ever contained in MainWindow 
        window.StartTraining(minEpochs, maxEpochs, maxError, batchesPerEpoch);
    }
}
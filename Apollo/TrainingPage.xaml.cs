using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Apollo.NeuralNet;

namespace Apollo;

public partial class TrainingPage : Page
{
    private NeuralNetwork Network { get; }
    private StoredSettings Settings { get; }
    
    public TrainingPage()
    {
        Network = (Application.Current as App).Network;
        Settings = (Application.Current as App).Settings;
        
        InitializeComponent();

        MinEpochSlider.Value = Settings.MinEpochs;
        MaxEpochSlider.Value = Settings.MaxEpochs;
        MaxErrorSlider.Value = Settings.MaxError;
        BatchesPerEpochSlider.Value = Settings.BatchesPerEpoch;
    }

    /// <summary>
    ///     Collect values from slider and send them to neural network
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

        Network.Train(minEpochs, maxEpochs, maxError, batchesPerEpoch);
    }

    private void RevertButtonClicked(object sender, RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show("Are you sure?", "Revert Training Confirmation",
            MessageBoxButton.YesNo);

        if (confirmation == MessageBoxResult.No)
            return;

        var reverted = Network.TryRevertTraining();

        if (!reverted)
            MessageBox.Show("The attempt to revert training failed");
    }
}
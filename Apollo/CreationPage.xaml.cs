using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Apollo;

public partial class CreationPage : Page
{
    public CreationPage()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Opens a SaveFileDialog for the user
    /// </summary>
    /// <returns>The result of the file dialog</returns>
    private string GetSavePath()
    {
        var dialog = new SaveFileDialog();
        dialog.Filter = "MIDI File|*.mid";
        dialog.Title = "Save the Generated Piece";

        if (dialog.ShowDialog() == true)
            return Path.GetFullPath(dialog.FileName);

        return "";
    }

    /// <summary>
    ///     Called when the create button is clicked
    /// </summary>
    private void CreateButtonClicked(object sender, RoutedEventArgs e)
    {
        // Get the generation length + bpm from the sliders + prompt the user for the save path
        var generationLength = Convert.ToInt32(GenerationLenSlider.Value);
        var bpm = Convert.ToInt32(BpmSlider.Value);
        var savePath = GetSavePath();

        // Is null or empty if the user closes the dialog rather than selecting a save path 
        if (string.IsNullOrEmpty(savePath))
            return;

        // Start generating with the provided parameters
        (Application.Current as App).Network.Generate(generationLength, bpm, savePath);
    }
}
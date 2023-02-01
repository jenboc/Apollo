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

    private string GetSavePath()
    {
        var dialog = new SaveFileDialog();
        dialog.Filter = "MIDI File|*.mid";
        dialog.Title = "Save the Generated Piece";
        
        if (dialog.ShowDialog() == true)
            return Path.GetFullPath(dialog.FileName);
        
        return "";
    }
    
    private void CreateButtonClicked(object sender, RoutedEventArgs e)
    {
        var generationLength = Convert.ToInt32(GenerationLenSlider.Value);
        var savePath = GetSavePath();

        if (string.IsNullOrEmpty(savePath))
            return;
        
        var window = (MainWindow)Window.GetWindow(this); 
        window.StartCreating(generationLength, savePath);
    }
}
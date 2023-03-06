using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apollo;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class StartupWindow : Window
{
    public StartupWindow()
    {
        InitializeComponent();
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;


        // Create new window, and make it the "main window" of the application
        Mouse.OverrideCursor = Cursors.Wait;
        var mainWindow = new MainWindow(Convert.ToString(button.Content));
        var app = (App)Application.Current;
        app.MainWindow = mainWindow;
        app.MainWindow.Show();
        Close();
    }
}
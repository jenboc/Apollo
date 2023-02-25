using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Apollo.IO;
using Apollo.MatrixMaths;
using Apollo.NeuralNet;

namespace Apollo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Objects used across all parts of the application
        /// </summary>
        public ProfileManager ProfileManagement { get; set; }

        public StoredSettings Settings { get; set; }
        public Random R { get; set; }
        public NeuralNetwork Network { get; set; }
        
        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            ProfileManagement = new ProfileManager(StoredSettings.ProfilesPath);
        }
        
        /// <summary>
        /// Handling unhandled exceptions
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Create messages for the logs and message box
            var boxMsg = $"Unhandled exception occurred: {e.Exception.Message}";
            var logMsg = boxMsg + $"\n{e.Exception.StackTrace}";
            
            // Log message + stack trace
            LogManager.WriteLine(logMsg);
            
            // Show message to the user 
            MessageBox.Show(boxMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
            Shutdown(); // Close the app since the exception may have an effect on how the app runs.
        }
    }
}
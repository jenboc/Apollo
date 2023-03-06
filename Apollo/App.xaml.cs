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
        private const int BATCH_SIZE = 4;
        private const int HIDDEN_SIZE = 32;

        /// <summary>
        /// Objects used across all parts of the application
        /// </summary>
        public ProfileManager ProfileManagement { get; set; }

        public StoredSettings Settings { get; set; }
        public Random R { get; }
        public NeuralNetwork Network { get; }

        public App()
        {
            // Add unhandled exception handler 
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            R = new Random();
            
            // Load settings from settings.json if it exists and is valid 
            try
            {
                Settings = StoredSettings.Load();
                Console.WriteLine(Settings.MinEpochs);
            }
            // If it does not exist or it isn't valid, start with fresh settings
            catch (Exception e)
            {
                LogManager.WriteLine($"Settings set to default:\n{e}");
                Settings = StoredSettings.Default();
                Settings.Save();
            }

            ProfileManagement = new ProfileManager(Settings.ProfilesPath);
            var initialProfile = ProfileManagement.GetProfile(Settings.SelectedProfileName);

            if (object.ReferenceEquals(initialProfile, null))
                initialProfile = ProfileManagement.Any();
            
            Network = new NeuralNetwork(initialProfile, HIDDEN_SIZE, BATCH_SIZE, R);
            
            Console.WriteLine(Settings.MinEpochs);
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

        /// <summary>
        /// Is called when the application is closed
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            try 
            {
                // Save settings
                Settings.Save();
            }
            finally
            { 
                base.OnExit(e);    
            }
        }
    }
}
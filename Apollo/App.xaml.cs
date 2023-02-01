using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Apollo.IO;

namespace Apollo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var boxMsg = $"Unhandled exception occurred: {e.Exception.Message}";
            var logMsg = boxMsg + $"\n{e.Exception.StackTrace}";
            
            LogManager.WriteLine(logMsg);
            MessageBox.Show(boxMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
            Shutdown();
        }
    }
}
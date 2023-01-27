using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apollo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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

            States startingState = States.Train;
            switch (button.Name)
            {
                case "TrainButton":
                    startingState = States.Train;
                    break;
                case "CreateButton":
                    startingState = States.Create;
                    break;
                case "ListenButton":
                    startingState = States.Listen;
                    break;
                case "SettingsButton":
                    startingState = States.Settings;
                    break;
            }
            
            // Create new window, and make it the "main window" of the application
            var mainWindow = new MainWindow(startingState);
            var app = (App)Application.Current;
            app.MainWindow = mainWindow;
            app.MainWindow.Show();
            Close();
        }
    }
}
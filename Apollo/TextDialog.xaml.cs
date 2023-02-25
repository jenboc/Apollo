using System.Windows;

namespace Apollo;

public partial class TextDialog : Window
{
    public string Value => ResponseBox.Text; 
    
    public TextDialog(string promptLabel)
    {
        InitializeComponent();
        PromptLabel.Content = promptLabel;
    }

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
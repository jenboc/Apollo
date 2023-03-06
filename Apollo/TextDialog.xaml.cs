using System.Windows;

namespace Apollo;

public partial class TextDialog : Window
{
    public TextDialog(string promptLabel)
    {
        InitializeComponent();
        PromptLabel.Content = promptLabel;
    }

    public string Value => ResponseBox.Text;

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
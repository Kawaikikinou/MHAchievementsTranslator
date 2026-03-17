using System.Windows;
using System.Windows.Input;

namespace AchievementTranslator.Views;

public partial class InputDialog : Window
{
    public string Result { get; private set; } = string.Empty;

    public InputDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => InputBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) => Confirm();
    private void CancelButton_Click(object sender, RoutedEventArgs e) { DialogResult = false; }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)  Confirm();
        if (e.Key == Key.Escape) { DialogResult = false; }
    }

    private void Confirm()
    {
        Result = InputBox.Text.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(Result)) return;
        DialogResult = true;
    }
}

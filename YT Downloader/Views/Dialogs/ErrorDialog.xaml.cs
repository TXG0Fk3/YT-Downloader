using Microsoft.UI.Xaml.Controls;

namespace YT_Downloader.Views.Dialogs
{
    public sealed partial class ErrorDialog : ContentDialog
    {
        public ErrorDialog(string errorMessage)
        {
            InitializeComponent();
            ErrorTextBox.Text = errorMessage;
        }
    }
}

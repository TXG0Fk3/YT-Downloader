using Microsoft.UI.Xaml.Controls;


namespace YT_Downloader.Views
{
    // Serve s� pra mostrar os erros na tela
    public sealed partial class ErrorPage : Page
    {
        public ErrorPage(string error)
        {
            this.InitializeComponent();
            errorTextBlock.Text = error;
        }
    }
}

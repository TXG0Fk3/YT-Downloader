using Microsoft.UI.Xaml.Controls;


namespace YT_Downloader.NavigationViewPages
{
    // Serve só pra mostrar os erros na tela
    public sealed partial class ErrorPage : Page
    {
        public ErrorPage(string error)
        {
            this.InitializeComponent();
            errorTextBlock.Text = error;
        }
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using YT_Downloader.ViewModels.Dialogs;

namespace YT_Downloader.Views.Dialogs
{
    public sealed partial class DetailsDialog : ContentDialog
    {
        private DetailsDialogViewModel ViewModel { get; set; }

        public DetailsDialog()
        {
            InitializeComponent();

            ViewModel = App.GetService<DetailsDialogViewModel>();
            DataContext = ViewModel;
        }

        private void UrlTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.LoadContentInfoCommand.Execute(null);
            }
        }
    }
}

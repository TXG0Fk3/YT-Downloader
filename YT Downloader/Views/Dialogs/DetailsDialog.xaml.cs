using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using YT_Downloader.ViewModels.Dialogs;

namespace YT_Downloader.Views.Dialogs
{
    public sealed partial class DetailsDialog : ContentDialog
    {
        public DetailsDialog()
        {
            InitializeComponent();
            DataContext = App.GetService<DetailsDialogViewModel>();
        }

        private void UrlTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (DataContext is DetailsDialogViewModel viewModel)
                    viewModel.LoadContentInfoCommand.Execute(null);
            }
        }
    }
}

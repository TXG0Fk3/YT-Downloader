using Microsoft.UI.Xaml.Controls;
using YT_Downloader.ViewModels.Dialogs;

namespace YT_Downloader.Views.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();
            DataContext = App.GetService<SettingsDialogViewModel>();
        }
    }
}

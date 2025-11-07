using Microsoft.UI.Xaml.Controls;
using YT_Downloader.Enums;
using YT_Downloader.Helpers.UI;
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

        private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SettingsDialogViewModel VM)
            {
                VM.SelectThemeCommand.Execute(null);
                RequestedTheme = ThemeHelper.ConvertThemeOptionToElementTheme(VM.SelectedThemeOption);
            }
        }

        private void ToggleSwitch_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (DataContext is SettingsDialogViewModel VM)
                VM.AlwaysAskWhereSaveCommand.Execute(null);
        }
    }
}

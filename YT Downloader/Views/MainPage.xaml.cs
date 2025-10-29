using Microsoft.UI.Xaml.Controls;
using YT_Downloader.Services;
using YT_Downloader.ViewModels;

namespace YT_Downloader.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.GetService<MainPageViewModel>();

            Loaded += (_, __) => App.GetService<DialogService>().Initialize(XamlRoot);
        }
    }
}

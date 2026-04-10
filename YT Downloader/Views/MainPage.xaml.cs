using Microsoft.UI.Xaml.Controls;
using YT_Downloader.Services;
using YT_Downloader.ViewModels;

namespace YT_Downloader.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            ViewModel = App.GetService<MainPageViewModel>();
            DataContext = ViewModel;
            Loaded += (_, __) => App.GetService<DialogService>().Initialize(XamlRoot);
        }
    }
}

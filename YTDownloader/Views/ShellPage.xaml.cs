using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using YTDownloader.Services;
using YTDownloader.ViewModels;

namespace YTDownloader.Views;

public sealed partial class ShellPage : Page
{
    public ShellPageViewModel ViewModel { get; set; }

    public ShellPage()
    {
        InitializeComponent();

        ViewModel = App.GetService<ShellPageViewModel>();
        DataContext = ViewModel;

#if DEBUG || DEBUG_UNPACKAGED
        DebugBadge.Visibility = Visibility.Visible;
#endif

        Loaded += (_, __) => App.GetService<DialogService>().Initialize(XamlRoot);
    }
}

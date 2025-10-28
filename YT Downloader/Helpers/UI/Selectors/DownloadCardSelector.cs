using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using YT_Downloader.ViewModels.Components;

namespace YT_Downloader.Helpers.UI.Selectors
{
    public partial class DonwloadCardSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate GroupTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) =>
            item is DownloadItemViewModel ? ItemTemplate : GroupTemplate;
    }
}
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace YT_Downloader.Helpers.UI
{
    public static class ThemeHelper
    {
        public static ElementTheme ConvertStringToElementTheme(string elementTheme) =>
            elementTheme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

        public static void ApplyTheme(Window window, ElementTheme theme)
        {
            if (window.Content is FrameworkElement rootElement)
                rootElement.RequestedTheme = theme;

            if (theme == ElementTheme.Default)
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;

            ApplyTitleBarColors(window.AppWindow.TitleBar, theme);
        }

        private static void ApplyTitleBarColors(AppWindowTitleBar titleBar, ElementTheme theme)
        {
            Color buttonHoverBackgroundColor = theme == ElementTheme.Dark
                ? Color.FromArgb(255, 61, 61, 61)
                : Colors.LightGray;

            Color foregroundColor = theme == ElementTheme.Dark ? Colors.White : Colors.Black;

            titleBar.ButtonHoverBackgroundColor = buttonHoverBackgroundColor;
            titleBar.ForegroundColor = foregroundColor;
            titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.ButtonHoverForegroundColor = foregroundColor;
        }
    }
}

using CefSharp;
using MahApps.Metro.Controls;
using Opeity.Properties;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Opeity {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public ObservableCollection<string> History { get; set; }

        private CefState _webSecurity = CefState.Enabled;
        private CefState _applicationCache = CefState.Disabled;
        private CefState _databases = CefState.Disabled;
        private CefState _localStorage = CefState.Disabled;
        private CefState _javascriptAccessClipboard = CefState.Disabled;
        private CefState _javascriptCloseWindows = CefState.Disabled;
        private CefState _fileAccessFromFileUrls = CefState.Disabled;
        private CefState _plugins = CefState.Disabled;
        private int _windowlessFrameRate = 60; // max ?? PCMR

        /**
         * CefState is 0-2 value: true, false or default. Since we are
         * going for simplistic privacy approach here this can be simplified
         * to a boolean true or false value..
         */
        public bool ConvertCefState(CefState state)
        {
            switch (state)
            {
                case CefState.Default:
                    return false;

                case CefState.Enabled:
                    return true;

                case CefState.Disabled:
                    return false;

                default:
                    return false;
            }
        }

        private CefState ConvertBooleanToCefState(bool state)
        {
            switch (state)
            {
                case true:
                    return CefState.Enabled;

                case false:
                    return CefState.Disabled;

                default:
                    return CefState.Disabled;
            }
        }

        public MainWindow() {
            InitializeComponent();

            Browser.LoadError += (sender, args) => {
                if (args.ErrorCode == CefErrorCode.Aborted)
                    return;

                // ToDo: Update this to do something actually useful
            };

            History = new ObservableCollection<string>();
            Browser.TitleChanged += Browser_TitleChanged;

            Browser.BrowserSettings.WebSecurity = _webSecurity;
            Browser.BrowserSettings.ApplicationCache = _applicationCache;
            Browser.BrowserSettings.Databases = _databases;
            Browser.BrowserSettings.LocalStorage = _localStorage;
            Browser.BrowserSettings.JavascriptAccessClipboard = _javascriptAccessClipboard;
            Browser.BrowserSettings.JavascriptCloseWindows = _javascriptCloseWindows;
            Browser.BrowserSettings.FileAccessFromFileUrls = _fileAccessFromFileUrls;
            Browser.BrowserSettings.Plugins = _plugins;
            Browser.BrowserSettings.WindowlessFrameRate = _windowlessFrameRate;
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            #region Favicon

            var fullFilePath = $"http://www.google.com/s2/favicons?domain_url={Browser.Address}";
            var uiIcon = new BitmapImage();

            uiIcon.BeginInit();
            uiIcon.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            uiIcon.EndInit();

            Favicon.Source = uiIcon;

            #endregion

            Title = Browser.Title;
        }

        private void Chrome_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        #region Window Buttons

        private void C_BTN_Refresh_Click(object sender, RoutedEventArgs e) {
            if (Browser.IsLoading) {
                Browser.Stop();

            } else {
                Browser.Reload();
            }
        }

        private void C_BTN_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (Flyouts.Items[0] is Flyout flyout) flyout.IsOpen = !flyout.IsOpen;
        }

        private void C_BTN_Main_Click(object sender, RoutedEventArgs e) {
            Browser.Address = Settings.Default.Home;
        }

        #endregion

        #region Properties

        private void PropWebSecurity_Checked(object sender, RoutedEventArgs e)
        {
            _webSecurity = ConvertBooleanToCefState(PropWebSecurity.IsChecked.Value);
        }

        private void PropApplicationCache_Checked(object sender, RoutedEventArgs e)
        {
            _applicationCache = ConvertBooleanToCefState(PropApplicationCache.IsChecked.Value);
        }

        private void PropDatabases_Checked(object sender, RoutedEventArgs e)
        {
            _databases = ConvertBooleanToCefState(PropDatabases.IsChecked.Value);
        }

        private void PropLocalStorage_Checked(object sender, RoutedEventArgs e)
        {
           _localStorage = ConvertBooleanToCefState(PropLocalStorage.IsChecked.Value);
        }

        private void PropJavascriptAccessClipboard_Checked(object sender, RoutedEventArgs e)
        {
           _javascriptAccessClipboard = ConvertBooleanToCefState(PropJavascriptAccessClipboard.IsChecked.Value);
        }

        private void PropJavascriptCloseWindows_Checked(object sender, RoutedEventArgs e)
        {
           _javascriptCloseWindows = ConvertBooleanToCefState(PropJavascriptCloseWindows.IsChecked.Value);
        }

        private void PropFileAccessFromFileUrls_Checked(object sender, RoutedEventArgs e)
        {
            _fileAccessFromFileUrls = ConvertBooleanToCefState(PropFileAccessFromFileUrls.IsChecked.Value);
        }

        private void PropPlugins_Checked(object sender, RoutedEventArgs e)
        {
            _plugins = ConvertBooleanToCefState(PropPlugins.IsChecked.Value);
        }

        #endregion
    }
}

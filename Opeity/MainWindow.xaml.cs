using CefSharp;
using MahApps.Metro.Controls;
using Opeity.Properties;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using Opeity.Handlers;

namespace Opeity {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public ObservableCollection<string> History { get; set; }

        private DownloadHandler downloadHandler;

        private CefState _webSecurity = CefState.Enabled;
        private CefState _applicationCache = CefState.Disabled;
        private CefState _databases = CefState.Disabled;
        private CefState _localStorage = CefState.Disabled;
        private CefState _javascriptAccessClipboard = CefState.Disabled;
        private CefState _javascriptCloseWindows = CefState.Disabled;
        private CefState _fileAccessFromFileUrls = CefState.Disabled;
        private CefState _plugins = CefState.Disabled;
        private CefState _loadImages = CefState.Enabled;
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

        public MainWindow(string url) {
            InitializeComponent();

            Browser.LoadError += (sender, args) => {
                if (args.ErrorCode == CefErrorCode.Aborted)
                    return;

                var errorBody =
                    $"<html><body bgcolor=\"white\"><h2>Failed to load URL {args.FailedUrl} <br /><br />Error: {args.ErrorText}<br />Code:{args.ErrorCode}</h2></body></html>";

                args.Frame.LoadStringForUrl(errorBody, args.FailedUrl);
            };

            History = new ObservableCollection<string>();
            Browser.TitleChanged += Browser_TitleChanged;
            downloadHandler = new DownloadHandler();
            Browser.DownloadHandler = downloadHandler;
            Browser.LifeSpanHandler = new LifespanHandler();
            Browser.RequestHandler = new RequestHandler();
            downloadHandler.OnDownloadUpdatedFired += DownloadHandler_OnDownloadUpdatedFired;

            Browser.BrowserSettings.WebSecurity = _webSecurity;
            Browser.BrowserSettings.ApplicationCache = _applicationCache;
            Browser.BrowserSettings.Databases = _databases;
            Browser.BrowserSettings.LocalStorage = _localStorage;
            Browser.BrowserSettings.JavascriptAccessClipboard = _javascriptAccessClipboard;
            Browser.BrowserSettings.JavascriptCloseWindows = _javascriptCloseWindows;
            Browser.BrowserSettings.FileAccessFromFileUrls = _fileAccessFromFileUrls;
            Browser.BrowserSettings.Plugins = _plugins;
            Browser.BrowserSettings.ImageLoading = _loadImages;
            Browser.BrowserSettings.WindowlessFrameRate = _windowlessFrameRate;

            PropWebSecurity.IsChecked = ConvertCefState(_webSecurity);
            PropApplicationCache.IsChecked = ConvertCefState(_applicationCache);
            PropDatabases.IsChecked = ConvertCefState(_databases);
            PropLocalStorage.IsChecked = ConvertCefState(_localStorage);
            PropJavascriptAccessClipboard.IsChecked = ConvertCefState(_javascriptAccessClipboard);
            PropJavascriptCloseWindows.IsChecked = ConvertCefState(_javascriptCloseWindows);
            PropFileAccessFromFileUrls.IsChecked = ConvertCefState(_fileAccessFromFileUrls);
            PropPlugins.IsChecked = ConvertCefState(_plugins);

            Browser.FrameLoadEnd += delegate { Application.Current.Dispatcher.BeginInvoke((Action)(() => Browser.Focus())); };
            Browser.Address = string.IsNullOrEmpty(url) ? Settings.Default.Home : url;
        }

        private void DownloadHandler_OnDownloadUpdatedFired(object sender, DownloadItem e)
        {
            if (!e.IsComplete || !e.IsCancelled)
            {

                return;
            }


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
            if (downloadHandler.FilesDownloading.Count > 0)
            {
                if (MessageBox.Show(
                        "Files are still downloading. Quitting Opeity will cancel them, are you sure you want to quit?", "Downloading Files..", 
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        #region Window Buttons

        private void C_BTN_Refresh_Click(object sender, RoutedEventArgs e) {
            if (Browser.IsLoading) {
                Browser.Stop();

            } else {
                Browser.Reload();
            }
        }

        private void C_BTN_Downloads_Click(object sender, RoutedEventArgs e)
        {
            if (Flyouts.Items[0] is Flyout flyout) flyout.IsOpen = !flyout.IsOpen;
        }

        private void C_BTN_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (Flyouts.Items[1] is Flyout flyout) flyout.IsOpen = !flyout.IsOpen;
        }

        private void C_BTN_Main_Click(object sender, RoutedEventArgs e) {
            Browser.Address = Settings.Default.Home;
        }

        #endregion

        #region Properties

        private void PropWebSecurity_Checked(object sender, RoutedEventArgs e)
        {
            _webSecurity = ConvertBooleanToCefState(PropWebSecurity.IsChecked != null && PropWebSecurity.IsChecked.Value);
            Browser.BrowserSettings.WebSecurity = _webSecurity;
        }

        private void PropApplicationCache_Checked(object sender, RoutedEventArgs e)
        {
            _applicationCache = ConvertBooleanToCefState(
                PropApplicationCache.IsChecked != null && PropApplicationCache.IsChecked.Value);

            Browser.BrowserSettings.ApplicationCache = _applicationCache;
        }

        private void PropDatabases_Checked(object sender, RoutedEventArgs e)
        {
            _databases = ConvertBooleanToCefState(
                PropDatabases.IsChecked != null && PropDatabases.IsChecked.Value);

            Browser.BrowserSettings.Databases = _databases;
        }

        private void PropLocalStorage_Checked(object sender, RoutedEventArgs e)
        {
           _localStorage = ConvertBooleanToCefState(
               PropLocalStorage.IsChecked != null && PropLocalStorage.IsChecked.Value);

            Browser.BrowserSettings.LocalStorage = _localStorage;
        }

        private void PropJavascriptAccessClipboard_Checked(object sender, RoutedEventArgs e)
        {
           _javascriptAccessClipboard = ConvertBooleanToCefState(
               PropJavascriptAccessClipboard.IsChecked != null && PropJavascriptAccessClipboard.IsChecked.Value);

            Browser.BrowserSettings.JavascriptAccessClipboard = _javascriptAccessClipboard;
        }

        private void PropJavascriptCloseWindows_Checked(object sender, RoutedEventArgs e)
        {
           _javascriptCloseWindows = ConvertBooleanToCefState(
               PropJavascriptCloseWindows.IsChecked != null && PropJavascriptCloseWindows.IsChecked.Value);

            Browser.BrowserSettings.JavascriptCloseWindows = _javascriptCloseWindows;
        }

        private void PropFileAccessFromFileUrls_Checked(object sender, RoutedEventArgs e)
        {
            _fileAccessFromFileUrls = ConvertBooleanToCefState(
                PropFileAccessFromFileUrls.IsChecked != null && PropFileAccessFromFileUrls.IsChecked.Value);

            Browser.BrowserSettings.FileAccessFromFileUrls = _fileAccessFromFileUrls;
        }

        private void PropPlugins_Checked(object sender, RoutedEventArgs e)
        {
            _plugins = ConvertBooleanToCefState(
                PropPlugins.IsChecked != null && PropPlugins.IsChecked.Value);

            Browser.BrowserSettings.Plugins = _plugins;
        }

        private void PropLoadImages_Checked(object sender, RoutedEventArgs e)
        {
            _loadImages = ConvertBooleanToCefState(
                PropLoadImages.IsChecked != null && PropLoadImages.IsChecked.Value);

            Browser.BrowserSettings.ImageLoading = _loadImages;
        }

        #endregion

        private void C_BTN_Main_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MessageBox.Show(
                    $"Would you like to change your homepage to\n\n{Browser.Address}", "Change Homepage?",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Settings.Default.Home = Browser.Address;
                Settings.Default.Save();
            }
        }

        private void Chrome_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

using CefSharp;
using MahApps.Metro.Controls;
using Opeity.Properties;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using AngleSharp.Parser.Html;
using CommandLine;
using IWshRuntimeLibrary;
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

        public static bool _forceSingleWindow;
        public static bool _appMode;

        private class Options
        {
            [Option('o', "open", Required = false, HelpText = "Url to open.")]
            public string OpenUrl { get; set; }

            [Option('a', "app", Required = false, HelpText = "Open Opiety in App mode")]
            public bool App { get; set; }
        }

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

                var errorBody =
                    $"<html><body bgcolor=\"white\"><h2>Failed to load URL {args.FailedUrl} <br /><br />Error: {args.ErrorText}<br />Code:{args.ErrorCode}</h2></body></html>";

                args.Frame.LoadStringForUrl(errorBody, args.FailedUrl);
            };
            
            downloadHandler         = new DownloadHandler();
            Browser.DownloadHandler = downloadHandler;
            Browser.LifeSpanHandler = new LifespanHandler();
            Browser.RequestHandler  = new RequestHandler();
            History                 = new ObservableCollection<string>();

            Browser.TitleChanged += Browser_TitleChanged;
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;

            downloadHandler.OnDownloadUpdatedFired += DownloadHandler_OnDownloadUpdatedFired;
            
            Browser.FrameLoadEnd += delegate { Application.Current.Dispatcher.BeginInvoke((Action)(() => Browser.Focus())); };

            Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs()).WithParsed(options =>
            {
                Browser.Address = (string.IsNullOrEmpty(options.OpenUrl)) ? Settings.Default.Home : options.OpenUrl;

                if (options.App)
                {
                    _webSecurity               = CefState.Enabled;
                    _applicationCache          = CefState.Enabled;
                    _databases                 = CefState.Enabled;
                    _localStorage              = CefState.Enabled;
                    _javascriptAccessClipboard = CefState.Enabled;
                    _javascriptCloseWindows    = CefState.Enabled;
                    _fileAccessFromFileUrls    = CefState.Disabled;
                    _plugins                   = CefState.Disabled;
                    _loadImages                = CefState.Enabled;
                    _forceSingleWindow         = true;
                    _appMode                   = true;

                    C_BTN_Main.Visibility      = Visibility.Collapsed;
                    C_BTN_Back.Visibility      = Visibility.Collapsed;
                    C_BTN_Refresh.Visibility   = Visibility.Collapsed;
                    C_BTN_Forward.Visibility   = Visibility.Collapsed;
                    C_BTN_Settings.Visibility  = Visibility.Collapsed;
                }
            });

            Browser.BrowserSettings.WebSecurity               = _webSecurity;
            Browser.BrowserSettings.ApplicationCache          = _applicationCache;
            Browser.BrowserSettings.Databases                 = _databases;
            Browser.BrowserSettings.LocalStorage              = _localStorage;
            Browser.BrowserSettings.JavascriptAccessClipboard = _javascriptAccessClipboard;
            Browser.BrowserSettings.JavascriptCloseWindows    = _javascriptCloseWindows;
            Browser.BrowserSettings.FileAccessFromFileUrls    = _fileAccessFromFileUrls;
            Browser.BrowserSettings.Plugins                   = _plugins;
            Browser.BrowserSettings.ImageLoading              = _loadImages;
            Browser.BrowserSettings.WindowlessFrameRate       = _windowlessFrameRate;

            PropWebSecurity.IsChecked               = ConvertCefState(_webSecurity);
            PropApplicationCache.IsChecked          = ConvertCefState(_applicationCache);
            PropDatabases.IsChecked                 = ConvertCefState(_databases);
            PropLocalStorage.IsChecked              = ConvertCefState(_localStorage);
            PropJavascriptAccessClipboard.IsChecked = ConvertCefState(_javascriptAccessClipboard);
            PropJavascriptCloseWindows.IsChecked    = ConvertCefState(_javascriptCloseWindows);
            PropFileAccessFromFileUrls.IsChecked    = ConvertCefState(_fileAccessFromFileUrls);
            PropPlugins.IsChecked                   = ConvertCefState(_plugins);
            PropLoadImages.IsChecked                = ConvertCefState(_loadImages);
            PropForceSingleWindow.IsChecked         = _forceSingleWindow;
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                Browser.GetSourceAsync().ContinueWith(taskHtml =>
                {
                    var html = taskHtml.Result;
                    var parser = new HtmlParser();
                    var document = parser.Parse(html);
                    var colorItems = document.All.Where(m => m.LocalName == "meta" && m.GetAttribute("name") == "theme-color");
                    var color = string.Empty;

                    foreach (var colorItem in colorItems)
                        color = colorItem.GetAttribute("content");

                    if (!string.IsNullOrEmpty(color))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            WindowTitleBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(color);
                        }));
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Brush defBrush = new SolidColorBrush((Color)FindResource("AccentColor"));

                            if (!Equals(WindowTitleBrush, defBrush))
                                WindowTitleBrush = defBrush;

                        }));
                    }
                });
            }
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

            var fullFilePath = $"https://api.statvoo.com/favicon/?url={Browser.Address}";
            var uiIcon = new BitmapImage();

            uiIcon.BeginInit();
            uiIcon.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            uiIcon.EndInit();

            Favicon.Source = uiIcon;
            Icon = uiIcon;

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

        private void C_BTN_MakeApp_Click(object sender, RoutedEventArgs e)
        {
            var appDir =
                Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Apps");

            if (!Directory.Exists(appDir))
                Directory.CreateDirectory(appDir);

            using (var client = new WebClient())
            {
                client.DownloadFile(
                    $"https://api.statvoo.com/favicon/?url={Browser.Address}",
                    Path.Combine(appDir, $"{Browser.Title}.ico")
                );
            }

            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var shell = new WshShell();
            var shortCutLinkFilePath = Path.Combine(startupFolderPath, $"{Browser.Title}.lnk");
            var windowsApplicationShortcut = (IWshShortcut)shell.CreateShortcut(shortCutLinkFilePath);
            windowsApplicationShortcut.Description = Browser.Title;

            windowsApplicationShortcut.WorkingDirectory =
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            windowsApplicationShortcut.TargetPath =
                System.Reflection.Assembly.GetExecutingAssembly().Location;

            windowsApplicationShortcut.Arguments = $"-app -o \"{Browser.Address}\"";
            windowsApplicationShortcut.IconLocation = Path.Combine(appDir, $"{Browser.Title}.ico");
            windowsApplicationShortcut.Save();
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

        private void PropForceSingleWindow_Checked(object sender, RoutedEventArgs e)
        {
            if (PropForceSingleWindow.IsChecked != null)
                _forceSingleWindow = PropForceSingleWindow.IsChecked.Value;
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

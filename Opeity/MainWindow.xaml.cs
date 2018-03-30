using AngleSharp.Parser.Html;
using CefSharp;
using CommandLine;
using IWshRuntimeLibrary;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Opeity.Handlers;
using Opeity.Objects;
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
using Squirrel;
using Application = System.Windows.Application;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Opeity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<string> History { get; set; }
        public ObservableCollection<DownloadItemEx> filesDownloading = new ObservableCollection<DownloadItemEx>();

        private DownloadHandler downloadHandler;

        private CefState _webSecurity;
        private CefState _applicationCache;
        private CefState _databases;
        private CefState _localStorage;
        private CefState _javascriptAccessClipboard;
        private CefState _javascriptCloseWindows;
        private CefState _fileAccessFromFileUrls;
        private CefState _plugins;
        private CefState _loadImages;
        private int _windowlessFrameRate;

        public static bool _forceSingleWindow;
        public static bool _appMode;

        private bool _forcedExit = false;

        #region Command Line Parser Options

        private class Options
        {
            [Option('o', "open", Required = false, HelpText = "Url to open.")]
            public string OpenUrl { get; set; }

            [Option('a', "app", Required = false, HelpText = "Open Opiety in App mode")]
            public bool App { get; set; }
        }

        #endregion

        #region Cef Converters

        /**
         * CefState is 0-2 value: true, false or default. Since we are
         * going for simplistic approach here this can be simplified
         * to a boolean true or false value..
         */
        public bool CefStateToBoolean(CefState state)
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

        private CefState BooleanToCefState(bool state)
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

        #endregion

        public MainWindow() {
            InitializeComponent();
            
            #region Browser Handlers

            downloadHandler         = new DownloadHandler();
            Browser.DownloadHandler = downloadHandler;
            Browser.LifeSpanHandler = new LifespanHandler();
            Browser.RequestHandler  = new RequestHandler();
            Browser.MenuHandler     = new ContextMenuHandler();
            Browser.RequestContext  = new RequestContext(new RequestContextHandler());
            History                 = new ObservableCollection<string>();
            Downloads.ItemsSource   = filesDownloading;

            #endregion

            #region Browser Events

            Browser.TitleChanged += Browser_TitleChanged;
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            Browser.LoadError += Browser_LoadError;

            downloadHandler.OnDownloadItemUpdated += DownloadHandlerOnOnDownloadItemUpdated;

            #endregion

            #region Load User Settings

            _webSecurity = BooleanToCefState(Settings.Default.WebSecurity);
            _applicationCache = BooleanToCefState(Settings.Default.ApplicationCache);
            _databases = BooleanToCefState(Settings.Default.Databases);
            _localStorage = BooleanToCefState(Settings.Default.LocalStorage);
            _javascriptAccessClipboard = BooleanToCefState(Settings.Default.JavascriptAccessClipboard);
            _javascriptCloseWindows = BooleanToCefState(Settings.Default.JavascriptCloseWindows);
            _fileAccessFromFileUrls = BooleanToCefState(Settings.Default.FileAccessFromFileUrls);
            _plugins = BooleanToCefState(Settings.Default.Plugins);
            _loadImages = BooleanToCefState(Settings.Default.LoadImages);
            _windowlessFrameRate = Settings.Default.WindowlessFrameRate;

            #endregion

            #region Command Line Parser

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
                    C_BTN_MakeApp.Visibility   = Visibility.Collapsed;
                }
            });

            #endregion

            #region Browser Defaults
            
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

            PropWebSecurity.IsChecked               = CefStateToBoolean(_webSecurity);
            PropApplicationCache.IsChecked          = CefStateToBoolean(_applicationCache);
            PropDatabases.IsChecked                 = CefStateToBoolean(_databases);
            PropLocalStorage.IsChecked              = CefStateToBoolean(_localStorage);
            PropJavascriptAccessClipboard.IsChecked = CefStateToBoolean(_javascriptAccessClipboard);
            PropJavascriptCloseWindows.IsChecked    = CefStateToBoolean(_javascriptCloseWindows);
            PropFileAccessFromFileUrls.IsChecked    = CefStateToBoolean(_fileAccessFromFileUrls);
            PropPlugins.IsChecked                   = CefStateToBoolean(_plugins);
            PropLoadImages.IsChecked                = CefStateToBoolean(_loadImages);
            PropForceSingleWindow.IsChecked         = _forceSingleWindow;

            #endregion
        }

        private void DownloadHandlerOnOnDownloadItemUpdated(object sender, DownloadHandler.DownloadProgress e)
        {
            if (e.Item.IsInProgress)
            {
                if ((!string.IsNullOrEmpty(e.Item.Url)) &&
                    (!string.IsNullOrEmpty(e.Item.OriginalUrl)) &&
                    (!string.IsNullOrEmpty(e.Item.FullPath)))
                {
                    var pass = true;
                    foreach (var download in filesDownloading)
                    {
                        if (download.Id == e.Item.Id)
                        {
                            download.File = Path.GetFileName(e.Item.FullPath);
                            download.Source = e.Item.Url;
                            download.Destination = e.Item.FullPath;
                            download.Speed = e.Item.CurrentSpeed;
                            download.Progress = e.Item.PercentComplete;
                            download.UpdateReadableSpeed();
                            pass = false;
                        }
                    }

                    if (pass)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            filesDownloading.Add(new DownloadItemEx(e.Item));
                        });
                    }
                }

                return;
            }

            if (e.Item.IsCancelled)
            {
                foreach (var download in filesDownloading)
                {
                    if (download.Id == e.Item.Id)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            filesDownloading.Remove(download);
                        });
                        
                        break;
                    }
                }
            }

            if (e.Item.IsComplete)
            {
                foreach (var download in filesDownloading)
                {
                    if (download.Id == e.Item.Id)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            filesDownloading.Remove(download);
                        });

                        break;
                    }
                }
            }
        }

        public async Task<bool> Prompt(string title, string message)
        {
            var result = await this.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
                return true;

            return false;
        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.ErrorCode == CefErrorCode.Aborted)
                return;

            var errorBody =
                $"<html><body bgcolor=\"white\"><pre>Failed to load URL {e.FailedUrl} | Error: {e.ErrorText} | Code:{e.ErrorCode}</pre></body></html>";

            e.Frame.LoadStringForUrl(errorBody, e.FailedUrl);
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => Browser.Focus()));

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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            WindowTitleBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(color);
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Brush defBrush = new SolidColorBrush((Color)FindResource("AccentColor"));

                            if (!Equals(WindowTitleBrush, defBrush))
                                WindowTitleBrush = defBrush;

                        });
                    }
                });
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

        private async void Chrome_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (filesDownloading.Count > 0)
            {
                if (!_forcedExit)
                    e.Cancel = true;

                var result = await Prompt("Downloading Files..",
                    "Files are still downloading. Quitting Opeity will cancel them, are you sure you want to quit?");

                if (result)
                {
                    _forcedExit = true;
                    Close();
                }
            }

            Browser.Dispose();
            Cef.Shutdown();

            #region Save User Settings

            Settings.Default.WebSecurity = CefStateToBoolean(_webSecurity);
            Settings.Default.ApplicationCache = CefStateToBoolean(_applicationCache);
            Settings.Default.Databases = CefStateToBoolean(_databases);
            Settings.Default.LocalStorage = CefStateToBoolean(_localStorage);
            Settings.Default.JavascriptAccessClipboard = CefStateToBoolean(_javascriptAccessClipboard);
            Settings.Default.JavascriptCloseWindows = CefStateToBoolean(_javascriptCloseWindows);
            Settings.Default.FileAccessFromFileUrls = CefStateToBoolean(_fileAccessFromFileUrls);
            Settings.Default.Plugins = CefStateToBoolean(_plugins);
            Settings.Default.LoadImages = CefStateToBoolean(_loadImages);
            Settings.Default.WindowlessFrameRate = _windowlessFrameRate;
            Settings.Default.Save();

            #endregion
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

        private async void C_BTN_Main_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var result = await Prompt("Change Homepage?",
                $"Would you like to change your homepage to\n\n{Browser.Address}");

            if (result)
            {
                Settings.Default.Home = Browser.Address;
                Settings.Default.Save();
            }
        }

        private void C_BTN_MakeApp_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                Title = Properties.Resources.MainWindow_C_BTN_MakeApp_Click_Create_App_Shortcut,
                AddExtension = true,
                CheckPathExists = true,
                FileName = $"{Browser.Title}.lnk",
                Filter = "Shortcut (*.lnk)|*.lnk",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
            };

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                
                var shell = new WshShell();
                var shortCutLinkFilePath = sfd.FileName;
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
        }

        #endregion

        #region Properties

        private void PropWebSecurity_Checked(object sender, RoutedEventArgs e)
        {
            _webSecurity = BooleanToCefState(PropWebSecurity.IsChecked != null && PropWebSecurity.IsChecked.Value);
            Browser.BrowserSettings.WebSecurity = _webSecurity;
        }

        private void PropApplicationCache_Checked(object sender, RoutedEventArgs e)
        {
            _applicationCache = BooleanToCefState(
                PropApplicationCache.IsChecked != null && PropApplicationCache.IsChecked.Value);

            Browser.BrowserSettings.ApplicationCache = _applicationCache;
        }

        private void PropDatabases_Checked(object sender, RoutedEventArgs e)
        {
            _databases = BooleanToCefState(
                PropDatabases.IsChecked != null && PropDatabases.IsChecked.Value);

            Browser.BrowserSettings.Databases = _databases;
        }

        private void PropLocalStorage_Checked(object sender, RoutedEventArgs e)
        {
           _localStorage = BooleanToCefState(
               PropLocalStorage.IsChecked != null && PropLocalStorage.IsChecked.Value);

            Browser.BrowserSettings.LocalStorage = _localStorage;
        }

        private void PropJavascriptAccessClipboard_Checked(object sender, RoutedEventArgs e)
        {
           _javascriptAccessClipboard = BooleanToCefState(
               PropJavascriptAccessClipboard.IsChecked != null && PropJavascriptAccessClipboard.IsChecked.Value);

            Browser.BrowserSettings.JavascriptAccessClipboard = _javascriptAccessClipboard;
        }

        private void PropJavascriptCloseWindows_Checked(object sender, RoutedEventArgs e)
        {
           _javascriptCloseWindows = BooleanToCefState(
               PropJavascriptCloseWindows.IsChecked != null && PropJavascriptCloseWindows.IsChecked.Value);

            Browser.BrowserSettings.JavascriptCloseWindows = _javascriptCloseWindows;
        }

        private void PropFileAccessFromFileUrls_Checked(object sender, RoutedEventArgs e)
        {
            _fileAccessFromFileUrls = BooleanToCefState(
                PropFileAccessFromFileUrls.IsChecked != null && PropFileAccessFromFileUrls.IsChecked.Value);

            Browser.BrowserSettings.FileAccessFromFileUrls = _fileAccessFromFileUrls;
        }

        private void PropPlugins_Checked(object sender, RoutedEventArgs e)
        {
            _plugins = BooleanToCefState(
                PropPlugins.IsChecked != null && PropPlugins.IsChecked.Value);

            Browser.BrowserSettings.Plugins = _plugins;
        }

        private void PropLoadImages_Checked(object sender, RoutedEventArgs e)
        {
            _loadImages = BooleanToCefState(
                PropLoadImages.IsChecked != null && PropLoadImages.IsChecked.Value);

            Browser.BrowserSettings.ImageLoading = _loadImages;
        }

        private void PropForceSingleWindow_Checked(object sender, RoutedEventArgs e)
        {
            if (PropForceSingleWindow.IsChecked != null)
                _forceSingleWindow = PropForceSingleWindow.IsChecked.Value;
        }

        #endregion
        
        private async void Chrome_Loaded(object sender, RoutedEventArgs e)
        {
            using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/Cryptobyte/Opeity-Cef"))
            {
                await mgr.Result.UpdateApp();
            }
        }
    }
}

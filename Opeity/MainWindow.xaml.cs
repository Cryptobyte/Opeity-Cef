using MahApps.Metro.Controls;
using Opeity.Properties;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Opeity {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        public MainWindow() {
            InitializeComponent();

            Browser.TitleChanged += Browser_TitleChanged;
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var fullFilePath = String.Format("http://www.google.com/s2/favicons?domain_url={0}", Browser.Address);

            BitmapImage UIIcon = new BitmapImage();
            UIIcon.BeginInit();
            UIIcon.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            UIIcon.EndInit();

            BitmapImage ChromeIcon = new BitmapImage();
            ChromeIcon.BeginInit();
            ChromeIcon.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            ChromeIcon.DecodePixelHeight = 32;
            ChromeIcon.DecodePixelWidth = 32;
            ChromeIcon.EndInit();

            Favicon.Source = UIIcon;
            Chrome.Icon = ChromeIcon;
        }

        #region Window Buttons

        private void C_BTN_Refresh_Click(object sender, RoutedEventArgs e) {
            if (Browser.IsLoading) {
                Browser.Stop();
            } else {
                if (Browser.CanReload)
                    Browser.Reload();
            }
        }

        private void C_BTN_Settings_Click(object sender, RoutedEventArgs e) {
            var flyout = this.Flyouts.Items[0] as Flyout;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void C_BTN_Main_Click(object sender, RoutedEventArgs e) {
            Browser.Address = Settings.Default.Home;
        }

        #endregion
    }
}

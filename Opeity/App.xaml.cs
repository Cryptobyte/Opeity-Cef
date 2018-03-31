using System.IO;
using CefSharp;
using System.Windows;
using Squirrel;

namespace Opeity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var activeDirectory =
                System.Reflection.Assembly.GetExecutingAssembly().Location;

            var widevineCdn =
                Path.Combine(activeDirectory, "WidevineCdm");

            var cefSettings = new CefSettings();
                cefSettings.CefCommandLineArgs.Add("--enable-widevine-cdm", "1");
                cefSettings.UserAgent = 
                    $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36 Opeity/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

            if (Directory.Exists(widevineCdn))
                Cef.RegisterWidevineCdm(widevineCdn);

            Cef.EnableHighDPISupport();
            Cef.Initialize(
                cefSettings, 
                performDependencyCheck: true, 
                browserProcessHandler: null
            );

            new MainWindow().Show();
        }
    }
}

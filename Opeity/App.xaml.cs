using CefSharp;
using CommandLine;
using Opeity.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Opeity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Command Line Parser Options

        private class Options
        {
            [Option('o', "open", Required = false, HelpText = "Url to open.")]
            public string OpenUrl { get; set; }

            [Option('a', "app", Required = false, HelpText = "Open Opiety in App mode")]
            public bool App { get; set; }
        }

        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool invalidApp = false;

            var activeDirectory =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var widevineCdn =
                Path.Combine(activeDirectory, "WidevineCdm");

            var cefSettings = new CefSettings();
                cefSettings.CefCommandLineArgs.Add("enable-media-stream", "1");
                cefSettings.CefCommandLineArgs.Add("enable-widevine-cdm", "1");
                cefSettings.CefCommandLineArgs.Add("no-proxy-server", "1");
                cefSettings.LogSeverity = LogSeverity.Disable;
                cefSettings.UserAgent =
                    $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36 Opeity/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            
            if (Directory.Exists(widevineCdn))
                Cef.RegisterWidevineCdm(widevineCdn);

            Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs()).WithParsed(options =>
            {
                if (options.App)
                {
                    var appHelper = new AppHelper();

                    if (!string.IsNullOrEmpty(options.OpenUrl))
                    {
                        var app = appHelper.Find(options.OpenUrl);

                        if (app != null)
                        {
                            cefSettings.CachePath = app.CachePath;
                        }
                        else
                        {
                            invalidApp = true;
                        }
                    }
                    else
                    {
                        invalidApp = true;
                    }
                }
            });

            Cef.EnableHighDPISupport();
            Cef.Initialize(
                cefSettings,
                performDependencyCheck: false,
                browserProcessHandler: null
            );

            new MainWindow(invalidApp).Show();
        }
    }
}

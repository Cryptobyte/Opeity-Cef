using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CommandLine;

namespace Opeity {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Cef.EnableHighDPISupport();
            Cef.Initialize(
                new CefSettings(), 
                performDependencyCheck: true, 
                browserProcessHandler: null
            );

            new MainWindow().Show();
        }
    }
}

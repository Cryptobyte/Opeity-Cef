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

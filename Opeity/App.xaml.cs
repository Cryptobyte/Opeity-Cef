using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Opeity {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Console.WriteLine((e.Args.Length > 0) ? e.Args[0] : "No?");
            new MainWindow((e.Args.Length > 0) ? e.Args[0] : null).Show();
        }
    }
}

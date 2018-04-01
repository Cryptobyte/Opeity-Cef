using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp.Wpf;
using Opeity.Helpers;

namespace Opeity.Components
{
    /// <summary>
    /// Interaction logic for CefWrapperControl.xaml
    /// </summary>
    public partial class CefWrapperControl
    {
        public ChromiumWebBrowser CefBrowser;

        public CefWrapperControl()
        {
            InitializeComponent();

            CefBrowser = Browser;
        }
    }
}
